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

def connect_to_existing_db():
    db_path = r"C:\Users\honlo\Documents\NailArtHub\NailArtHub.db"
    
    print(f"正在連線到現有的資料庫: {db_path}")
    conn = sqlite3.connect(db_path)
    return conn

edge_service = Service(EdgeChromiumDriverManager().install())
driver = webdriver.Edge(service=edge_service) 
driver.maximize_window()

if len(sys.argv) > 1:
    target_tag = sys.argv[1]
    search_keyword = f"{target_tag} nails trend"
else:
    target_tag = "Cateyes"
    search_keyword = "Cateyes nails trend"

print(f"Python 收到指令！準備開始爬取關鍵字: {search_keyword}")

driver.get(f"https://www.pinterest.com/search/pins/?q={search_keyword}")

try:
    print(f"成功進入 Pinterest，準備將資料存入現有的 NailTrend 資料表...")
    db_conn = connect_to_existing_db()
    cursor = db_conn.cursor()

    WebDriverWait(driver, 15).until(
        EC.presence_of_element_located((By.TAG_NAME, 'img'))
    )
    
    print("網頁載入成功，向下滾動以載入更多美甲圖片...")
    for i in range(2):
        driver.execute_script("window.scrollTo(0, document.body.scrollHeight);")
        time.sleep(3)

    print("開始抓取美甲圖片並寫入資料庫...")
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

                cursor.execute('''
                    INSERT INTO NailTrend (Tag, Title, ImageUrl, SourceUrl, CrawledAt)
                    VALUES (?, ?, ?, ?, ?)
                ''', (target_tag, title, img_url, source_url, crawled_at))
                saved_count += 1
                print(f"成功抓取第 {saved_count} 張圖片: {title[:20]}...")
                
        if saved_count >= 15:
            break

    db_conn.commit()
    db_conn.close()
    print(f"🎉 大功告成！Python 成功把 {saved_count} 筆新資料塞進你原有的 SQLite 資料庫囉！")

except Exception as e:
    print(f"❌ 發生錯誤: {e}")

finally:
    print("關閉瀏覽器")
    driver.quit()