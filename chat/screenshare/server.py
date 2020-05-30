# hackathon quality -- don't mind us
import datetime
import hashlib
import re
import socket
import _thread
import threading
import time

latestImage = {}

incomingVideoEndpoint = ("127.0.0.1", 15050)  # stunnel only needs access
outgoingVideoEndpoint = ("127.0.0.1", 5050)   # bind just to local.  gets proxied via nginx

with open("nostream.jpg", mode="rb") as file:
    defaultBytes = file.read()

defaultImage = (defaultBytes, hashlib.md5(defaultBytes).digest())

class SocketServer(object):
    def __init__(self, host, port):
        self.host = host
        self.port = port
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.sock.bind((self.host, self.port))

    def listen(self):
        self.sock.listen(10)
        while True:
            client, address = self.sock.accept()
            client.settimeout(60)
            threading.Thread(target = self.handleRequest, args = (client,address)).start()

    def handleRequest(self, client, address):
        pass


class IncomingVideoTcpHandler(SocketServer):
    def handleRequest(self, client, address):
        print("incoming video!")
        global latestImage
        global defaultImage

        clientSending = True
        while clientSending:
            header = b''
            # get the header
            while (len(header) != 48):
                data = client.recv(48 - len(header))
                if not data:
                    clientSending = False
                    break

                header = header + data

            # get the image
            jpeg = b''
            if clientSending:
                asciiGuid = header[0:36].decode("utf-8")
                jpegLength = int(header[37:47].decode("utf-8"))

                while (len(jpeg) != jpegLength):
                    data = client.recv(jpegLength - len(jpeg))
                    if not data:
                        clientSending = False
                        break

                    jpeg = jpeg + data

            if jpeg:
                newHash = hashlib.md5(jpeg).digest()
                if newHash == latestImage.get(asciiGuid, (None, None))[1]:
                    continue

                latestImage[asciiGuid] = (jpeg, newHash)
            else:
                print("incoming stream ending.  using default image")
                latestImage[asciiGuid] = defaultImage

        client.close()

class OutgoingVideoTcpHandler(SocketServer):
    def handleRequest(self, client, address):
        global latestImage
        global defaultImage

        print("outgoing video!")
        request = client.recv(4096)
        print(request)

        requestStr = request.decode("utf-8")
        m = re.search('id=(.*) HTTP', requestStr)
        if not m:
            str = "HTTP/1.1 404 Not Found\r\n\r\n"
            client.sendall(str.encode('utf-8'))

            return

        asciiGuid = m.group(1)
        print(f"viewer request for {asciiGuid}")

        lastFrame = latestImage.get(asciiGuid, None)
        if (not lastFrame):
            latestImage[asciiGuid] = defaultImage

        str = "HTTP/1.1 200 OK\r\n"
        str += 'Content-Type: multipart/x-mixed-replace; boundary=HACKATHON_XYZ\r\n'
        str += "\r\n"
        client.sendall(str.encode('utf-8'))

        lastHashSent = ""
        lastSendTime = datetime.datetime.now()
        while True:
            lastFrame, lastHash = latestImage[asciiGuid]

            # seems off ..
            if (lastHashSent == lastHash and (datetime.datetime.now() - lastSendTime)/datetime.timedelta(milliseconds=1) < 500):
                continue

            lastHashSent = lastHash
            lastSendTime = datetime.datetime.now()

            str = "--HACKATHON_XYZ\r\n"
            str += "Content-Type: image/jpeg\r\n"
            str += f"Content-Length: {len(lastFrame)}\r\n\r\n"
            client.sendall(str.encode('utf-8'))

            client.sendall(lastFrame)

            client.sendall("\r\n\r\n".encode('utf-8'))

            # print("sent new frame!")

def outgoingVideoThread(host, port):
    server = OutgoingVideoTcpHandler(host, port)
    server.listen()

def incomingVideoThread(host, port):
    server = IncomingVideoTcpHandler(host, port)
    server.listen()

if __name__ == "__main__":
    threading.Thread(target = outgoingVideoThread, args = outgoingVideoEndpoint).start()
    threading.Thread(target = incomingVideoThread, args = incomingVideoEndpoint).start()

    # just to keep the program up.  is there a better way?
    while 1:
        pass
