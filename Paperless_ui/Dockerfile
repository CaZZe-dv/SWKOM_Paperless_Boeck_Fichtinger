FROM nginx:alpine

# Kopiere die nginx-Konfigurationsdatei
COPY nginx.conf /etc/nginx/nginx.conf

# Kopiere den UI-Code in das Verzeichnis f�r statische Dateien
COPY ui/ /usr/share/nginx/html/

# Exponiere den Port 80, auf dem Nginx l�uft
EXPOSE 80
