import sys
import time
import sqlite3
from datetime import datetime
from selenium import webdriver
from selenium.webdriver.edge.service import Service 
from webdriver_manager.microsoft import EdgeChromiumDriverManager 
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.edge.options import Options

def connect_to_existing_db():
    db_path = r"C:\Users\honlo\Documents\NailArtHub\NailArtHub.db"
    print(f"Connecting to database: {db_path}")
    conn = sqlite3.connect(db_path, timeout=30)
    return conn

# if len(sys.argv) > 1:
#     target_tag = sys.argv[1].lower().replace(" ", "")
# else:
#     target_tag = "cateyes"
if len(sys.argv) > 1:
    target_tag = sys.argv[1].strip()
else:
    target_tag = "cateyes"

search_keyword = f"{target_tag} nail art trend"

print(f"Python trigger activated. Target Tag to save in DB: {target_tag}")
print(f"Search Keyword on Pinterest: {search_keyword}")

edge_options = Options()
edge_options.add_argument("--headless=new")
edge_options.add_argument("--disable-gpu")
edge_options.add_argument("--no-sandbox")
edge_options.add_argument("--disable-dev-shm-usage")
edge_options.add_argument("--no-first-run")
edge_options.add_argument("--no-default-browser-check")
edge_options.add_argument("--disable-features=EdgeShopping")
edge_options.add_argument("--blink-settings=imagesEnabled=false") # 不載入圖片加速爬取

edge_service = Service(EdgeChromiumDriverManager().install())
edge_service.creation_flags = 0x08000000 

driver = webdriver.Edge(service=edge_service, options=edge_options)
driver.set_page_load_timeout(20)

driver.get(f"https://www.pinterest.com/search/pins/?q={search_keyword}")

try:
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
    
    saved_count = 0
    for img in images:
        img_url = img.get_attribute('src')
        alt_text = img.get_attribute('alt')
        
        if img_url and ("236x" in img_url or "736x" in img_url):
            title = alt_text if alt_text else f"Stunning {target_tag} Nail Design"
            source_url = driver.current_url 
            crawled_at = datetime.now().strftime('%Y-%m-%d %H:%M:%S')

            cursor.execute("SELECT 1 FROM NailTrend WHERE ImageUrl = ?", (img_url,))
            if cursor.fetchone() is None:
                
                clean_tag = target_tag.lower().replace(" ", "").replace("#", "")
                cursor.execute('''
                    INSERT INTO NailTrend (Tag, Title, ImageUrl, SourceUrl, CrawledAt)
                    VALUES (?, ?, ?, ?, ?)
                ''', (target_tag, title, img_url, source_url, crawled_at))
                saved_count += 1
                print(f"Successfully got img No. {saved_count} : {title[:20]}...")
                
        if saved_count >= 15:
            break

    db_conn.commit()
    db_conn.close()
    print(f"Finished! Python successfully put {saved_count} new data into SQLite!")

except Exception as e:
    print(f"Selenium Error: {e}")

finally:
    print("Close the browser")
    driver.quit()