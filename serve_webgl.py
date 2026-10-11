import http.server
import socketserver
import os
import sys

PORT = 8088
WEBGL_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), "Build", "WebGL"))

class BrotliHTTPRequestHandler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=WEBGL_DIR, **kwargs)

    def end_headers(self):
        self.send_header('Cache-Control', 'no-cache, no-store, must-revalidate')
        self.send_header('Pragma', 'no-cache')
        self.send_header('Expires', '0')
        self.send_header('Access-Control-Allow-Origin', '*')
        clean_path = self.path.split('?')[0].split('#')[0]
        if clean_path.endswith('.wasm.br'):
            self.send_header('Content-Encoding', 'br')
            self.send_header('Content-Type', 'application/wasm')
        elif clean_path.endswith('.framework.js.br') or clean_path.endswith('.js.br'):
            self.send_header('Content-Encoding', 'br')
            self.send_header('Content-Type', 'application/javascript')
        elif clean_path.endswith('.data.br'):
            self.send_header('Content-Encoding', 'br')
            self.send_header('Content-Type', 'application/octet-stream')
        super().end_headers()

class ThreadingTCPServer(socketserver.ThreadingMixIn, socketserver.TCPServer):
    allow_reuse_address = True
    daemon_threads = True

if __name__ == '__main__':
    print(f"Serving WebGL from {WEBGL_DIR} on http://localhost:{PORT} ...")
    with ThreadingTCPServer(("", PORT), BrotliHTTPRequestHandler) as httpd:
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            pass

