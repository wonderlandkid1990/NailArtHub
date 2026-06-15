import os
import sys
import time
import pymssql
from datetime import datetime
from flask import Flask, request, jsonify
from selenium import webdriver
from selenium.webdriver.chrome.service import Service as ChromeService
from selenium.webdriver.chrome.options import Options as ChromeOptions
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from webdriver_manager.chrome import ChromeDriverManager

app = Flask(__name__)

def connect_to_existing_db():
    print("Connecting to Cloud SQL Server via pymssql...")
    conn = pymssql.connect(
        server='sql6034.site4now.net',
        user='db_aca7c2_nail_admin',
        password='Aliceyu19901103',
        database='db_aca7c2_nail',
        timeout=30
    )
    return conn

def run_crawler(target_tag):
    chrome_options = ChromeOptions()
    chrome_options.add_argument("--headless=new")
    chrome_options.add_argument("--no-sandbox")
    chrome_options.add_argument("--disable-dev-shm-usage")
    chrome_options.add_argument("--disable-gpu")
    chrome_options.add_argument("--blink-settings=imagesEnabled=false")

    print("Starting Chrome Driver for cloud environment...")
    driver = webdriver.Chrome(service=ChromeService(ChromeDriverManager().install()), options=chrome_options)
    driver.set_page_load_timeout(20)

    search_keyword = f"{target_tag} nail art trend"
    print(f"Python trigger activated. Target Tag to save in DB: {target_tag}")
    print(f"Search Keyword on Pinterest: {search_keyword}")

    saved_count = 0
    try:
        driver.get(f"https://www.pinterest.com/search/pins/?q={search_keyword}")
        print(f"Successfully entered Pinterest. Starting data harvest...")
        
        db_conn = connect_to_existing_db()
        cursor = db_conn.cursor()

        WebDriverWait(driver, 15).until(
            EC.presence_of_element_located((By.TAG_NAME, 'img'))
        )
        
        print("Successfully loaded the web, scrolling down for more images...")
        for i in range(2):
            driver.execute_script("window.scrollTo(0, document.body.scrollHeight);")
            time.sleep(2)

        print("Start to scraping and loading into the database...")
        images = driver.find_elements(By.TAG_NAME, 'img')
        
        for img in images:
            img_url = img.get_attribute('src')
            alt_text = img.get_attribute('alt')
            
            if img_url and ("236x" in img_url or "736x" in img_url):
                title = alt_text if alt_text else f"Stunning {target_tag} Nail Design"
                source_url = driver.current_url 
                crawled_at = datetime.now().strftime('%Y-%m-%d %H:%M:%S')

                cursor.execute("SELECT 1 FROM NailTrends WHERE ImageUrl = ?", (img_url,))
                if cursor.fetchone() is None:
                    clean_tag = target_tag.lower().replace(" ", "").replace("#", "")
                    cursor.execute('''
                        INSERT INTO [NailTrends] (Tag, Title, ImageUrl, SourceUrl, CrawledAt)
                        VALUES (?, ?, ?, ?, ?)
                    ''', (clean_tag, title, img_url, source_url, crawled_at))
                    saved_count += 1
                    print(f"Successfully got img No. {saved_count} : {title[:20]}...")
                    
            if saved_count >= 15:
                break

        db_conn.commit()
        db_conn.close()
        print(f"Finished! Python successfully put {saved_count} new data into SQL Server!")

    except Exception as e:
        print(f"Selenium Error: {e}")
        raise e
    finally:
        print("Close the browser")
        driver.quit()
        
    return saved_count

@app.route('/crawl', methods=['GET'])
def crawl_api():
    target_tag = request.args.get('tag', 'cateyes').strip()
    
    try:
        count = run_crawler(target_tag)
        return jsonify({
            "status": "success",
            "message": f"Successfully crawled and saved {count} items for tag '{target_tag}'."
        }), 200
    except Exception as e:
        return jsonify({
            "status": "error",
            "message": str(e)
        }), 500

@app.route('/', methods=['GET'])
def index():
    return "NailArtHub Python API Crawler is running successfully!"

if __name__ == '__main__':
    port = int(os.environ.get('PORT', 8080))
    app.run(host='0.0.0.0', port=port)