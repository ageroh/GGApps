from htmldom import htmldom
import re, pymssql, random

def get_ios_app_versions():
	ios_page = htmldom.HtmlDom("https://itunes.apple.com/gr/artist/travel-applications/id668571732").createDom()
	ios_rows = ios_page.find("a.artwork-link")
	ios_app_url_fmt = re.compile(r"https://itunes.apple.com/gr/app/[a-z\-]+-by-greekguide.com/.+")
	ios_app_urls = [m.group(0) for m in (re.search(ios_app_url_fmt, r.attr('href')) for r in ios_rows) if m]
	ret = []

	for ios_app_url in ios_app_urls:
		ios_app_page =  htmldom.HtmlDom(ios_app_url+"&rand="+str(random.randint(1000, 9999))).createDom()
		ios_app_name = ios_app_page.find("h1").text().split(' ')[0].lower()
		if len(ios_app_name) == 0:
			continue
		ios_app_version_raw =  ios_app_page.find("div#left-stack > div.lockup > ul.list > li")[3].text()
		ios_app_version = re.findall(r'\d\.\d\.?\d?', ios_app_version_raw)[0]
		ret.append((ios_app_name, ios_app_version))

	return ret

def get_android_app_versions():
	android_page = htmldom.HtmlDom("https://play.google.com/store/apps/developer?id=Travel%20Applications%20LTD&hl=en").createDom()
	android_rows = android_page.find("div.card-list div.details a.title")
	android_app_url_fmt = re.compile(r"/store/apps/details\?id=com\.greekguide\.apps\.android\.[a-zA-Z\-]+")
	android_app_urls = ["https://play.google.com"+m.group(0) for m in (re.search(android_app_url_fmt, r.attr('href')) for r in android_rows) if m]
	ret = []

	for android_app_url in android_app_urls:
		android_app_page =  htmldom.HtmlDom(android_app_url+"&rand="+str(random.randint(1000, 9999))).createDom()
		android_app_name = android_app_page.find("div[itemprop=name]").text().split(' ')[0].lower()
		if len(android_app_name) == 0:
			continue
		android_app_version_raw = android_app_page.find("div[itemprop=softwareVersion]").text()
		android_app_version = re.findall(r'\d\.\d\.?\d?', android_app_version_raw)[0]
		ret.append((android_app_name, android_app_version))

	return ret

conn = pymssql.connect("10.0.64.32", "ContentAbility_User_165", "3E6EA993-5EBA-4648-BF18-83C38D3E26DC", "GG_Reporting")

cursor = conn.cursor()

for app_version in get_ios_app_versions():
	cursor.callproc("dbo.usp_Check_and_Update_App_Version", (app_version[0],app_version[1],'iOS',))

for app_version in get_android_app_versions():
	cursor.callproc('usp_Check_and_Update_App_Version', (app_version[0],app_version[1],'Android',))

conn.commit()