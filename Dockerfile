FROM python:3.10-slim



RUN apt-get update && apt-get install -y --no-install-recommends \

gcc \

python3-dev \

&& apt-get clean \

&& rm -rf /var/lib/apt/lists/*



WORKDIR /app



COPY requirements.txt .

RUN pip install --no-cache-dir -r requirements.txt



COPY . .



CMD ["python", "import_sqlite3.py"]