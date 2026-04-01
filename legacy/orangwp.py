import os
import ctypes
import requests
import random
import customtkinter as ctk
from tkinter import filedialog
import threading
import re

def download_wallpaper(url, save_dir):
    try:
        os.makedirs(save_dir, exist_ok=True)
        random_number = random.randint(1000, 9999)
        extension = url.split('.')[-1].lower()
        filename = f"img{random_number}.{extension}"
        save_path = os.path.join(save_dir, filename)

        headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
        }
        response = requests.get(url, headers=headers, stream=True, timeout=10)
        response.raise_for_status()
        
        with open(save_path, 'wb') as file:
            for chunk in response.iter_content(chunk_size=8192):
                if chunk:
                    file.write(chunk)
        return save_path
    except Exception as e:
        print(f"Error downloading wallpaper: {e}")
        return None

# Function to set wallpaper
def set_wallpaper(path):
    try:
        if os.path.exists(path):
            SPI_SETDESKWALLPAPER = 20
            ctypes.windll.user32.SystemParametersInfoW(SPI_SETDESKWALLPAPER, 0, path, 3)
            return True
        return False
    except Exception as e:
        print(f"Error setting wallpaper: {e}")
        return False
def apply_wallpaper(url_or_path):
    save_dir = os.path.join(os.getenv('APPDATA'), "InterJava-Programs", "wallpaperchng")

    if os.path.exists(url_or_path):
        success = set_wallpaper(url_or_path)
        print("Local wallpaper set:", "Success" if success else "Failed")
    else:
        downloaded_path = download_wallpaper(url_or_path, save_dir)
        if downloaded_path:
            success = set_wallpaper(downloaded_path)
            print("Downloaded wallpaper set:", "Success" if success else "Failed")
        else:
            print("Failed to download wallpaper")
def is_valid_url(url):
    pattern = r'^(https?://)?(www\.)?[a-zA-Z0-9-]+(\.[a-zA-Z]{2,})+(\/[^\s]*)?$'
    return re.match(pattern, url) is not None
def create_gui():
    def show_loading_indicator():
        loading_label = ctk.CTkLabel(frame, text="Loading...")
        loading_label.pack(pady=5)
        root.update()
        return loading_label
    def hide_loading_indicator(loading_label):
        loading_label.destroy()
        root.update()
    ctk.set_appearance_mode("System")
    ctk.set_default_color_theme("blue")
    root = ctk.CTk()
    root.title("OrangWallpapers Changer")
    icon_path = os.path.join(os.path.dirname(__file__), "orange.ico")
    if os.path.exists(icon_path):
        root.iconbitmap(icon_path)
    else:
        print(f"Icon not found: {icon_path}")
    root.geometry("420x350")
    frame = ctk.CTkFrame(root)
    frame.pack(pady=20, padx=20, fill="both", expand=True)
    url_frame = ctk.CTkFrame(frame)
    url_frame.pack(pady=10, padx=10, fill="x")
    label = ctk.CTkLabel(url_frame, text="Enter Wallpaper URL:")
    label.pack(pady=5)
    url_entry = ctk.CTkEntry(url_frame, width=300)
    url_entry.pack(pady=5)
    def change_wallpaper_from_url():
        url = url_entry.get().strip()
        if url and is_valid_url(url):
            loading = show_loading_indicator()
            def task():
                apply_wallpaper(url)
                hide_loading_indicator(loading)

            threading.Thread(target=task).start()
        else:
            print("Invalid URL")
    change_button = ctk.CTkButton(
        url_frame, text="Change Wallpaper", command=change_wallpaper_from_url,
        fg_color="orange", text_color="white", hover_color="darkorange"
    )
    change_button.pack(pady=5)
    file_frame = ctk.CTkFrame(frame)
    file_frame.pack(pady=10, padx=10, fill="x")
    
    def select_local_file():
        file_path = filedialog.askopenfilename(
            title="Select Wallpaper",
            filetypes=[("Image Files", "*.jpg *.jpeg *.png *.bmp"), ("All Files", "*.*")]
        )
        if file_path:
            loading = show_loading_indicator()
            def task():
                apply_wallpaper(file_path)
                hide_loading_indicator(loading)
            threading.Thread(target=task).start()
    file_select_button = ctk.CTkButton(
        file_frame, text="Select Local Wallpaper", command=select_local_file,
        fg_color="orange", text_color="white", hover_color="darkorange"
    )
    file_select_button.pack(pady=10)
    predefined_frame = ctk.CTkFrame(frame)
    predefined_frame.pack(pady=10, padx=10, fill="x")
    predefined_label = ctk.CTkLabel(predefined_frame, text="Select a Predefined Wallpaper:")
    predefined_label.pack(pady=5)
    wallpaper_urls = {
        "Windows Default Wallpaper.": "C:\\Windows\\Web\\Wallpaper\\Windows\\img19.jpg",
        "Orange OS Wallpaper": "https://raw.githubusercontent.com/InterJavas-Projects/OrangWallpapers/main/wallpapers/img0.png",
        "Windows 11 365 White Ver.": "https://raw.githubusercontent.com/InterJavas-Projects/OrangWallpapers/main/wallpapers/img9989.jpg",
        "Windows Server 2025 White Ver.": "https://raw.githubusercontent.com/InterJavas-Projects/OrangWallpapers/main/wallpapers/img1993.jpg",
        "Captured Motion Black Ver.": "C:\\Windows\\Web\\Wallpaper\\ThemeB\\img27.jpg",
        "Flow Wallpaper White Ver.": "C:\\Windows\\Web\\Wallpaper\\ThemeD\\img32.jpg",
        "Windows 11 365 Black Ver.": "https://raw.githubusercontent.com/InterJavas-Projects/OrangWallpapers/main/wallpapers/img9990.jpg",
        "Windows Server 2025 Black Ver.": "https://raw.githubusercontent.com/InterJavas-Projects/OrangWallpapers/main/wallpapers/img4783.jpg",
        "Dragon Ball Z Son Goku Island": "https://raw.githubusercontent.com/InterJavas-Projects/OrangWallpapers/main/wallpapers/img6141.jpg",
        "CSGO AK 47 Gun Wallpaper": "https://raw.githubusercontent.com/InterJavas-Projects/OrangWallpapers/main/wallpapers/img1945.jpg",
        "Blank - Best For Performance": "https://image-0.uhdpaper.com/wallpaper/windows-11-365-abstract-dark-background-digital-art-2k-wallpaper-uhdpaper.com-549@0@i.jpg",
        "Forest White Blue": "https://raw.githubusercontent.com/InterJavas-Projects/OrangWallpapers/main/wallpapers/img2856.jpg",
        "Forest Purple Yellow": "https://raw.githubusercontent.com/InterJavas-Projects/OrangWallpapers/main/wallpapers/img8348.jpg",
        "Windows XP Bliss": "https://raw.githubusercontent.com/InterJavas-Projects/OrangWallpapers/main/wallpapers/img1119.jpg",
        "Simple Car Wallpaper": "https://raw.githubusercontent.com/InterJavas-Projects/OrangWallpapers/main/wallpapers/img4598.jpg",
        "Windows 10 Classic Wallpaper": "https://raw.githubusercontent.com/InterJavas-Projects/OrangWallpapers/main/wallpapers/img3106.jpg",
        "Windows 10 Classic Wallpaper + LTT": "https://raw.githubusercontent.com/InterJavas-Projects/OrangWallpapers/main/wallpapers/img9988.jpg",
        "Huskey Dog Wallpaper": "https://raw.githubusercontent.com/InterJavas-Projects/OrangWallpapers/main/wallpapers/img9577.jpg",
        "Windows SE Wallpaper": "https://raw.githubusercontent.com/InterJavas-Projects/OrangWallpapers/main/wallpapers/img9991.jpg",
        "Windows 10 Beach But Minecrafted": "https://raw.githubusercontent.com/InterJavas-Projects/OrangWallpapers/main/wallpapers/img9992.jpg",
    }
    def select_predefined_wallpaper():
        selection = wallpaper_optionmenu.get()
        if selection and selection in wallpaper_urls:
            url = wallpaper_urls[selection]
            loading = show_loading_indicator()
            def task():
                apply_wallpaper(url)
                hide_loading_indicator(loading)
            threading.Thread(target=task).start()
    wallpaper_optionmenu = ctk.CTkOptionMenu(
        predefined_frame, values=list(wallpaper_urls.keys()), command=lambda _: select_predefined_wallpaper(),
        fg_color="orange", text_color="white", button_color="orange", button_hover_color="darkorange"
    )
    wallpaper_optionmenu.pack(pady=10)
    root.mainloop()
create_gui()
