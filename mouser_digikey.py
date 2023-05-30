import requests
from json import dumps
from traceback import format_exc
import cv2
import easyocr
from time import sleep
from PIL import ImageDraw
# import pytesseract
# from pytesseract import Output
from PIL import Image
import matplotlib.pyplot as plt

#curl -X POST "https://api.mouser.com/api/v1/search/partnumber?apiKey=75cebc2d-2072-4155-910e-76d161912c9e" -H  "accept: application/json" -H  "Content-Type: application/json" -d "{  \"SearchByPartRequest\": {    \"mouserPartNumber\": \"PIC18F25K22-I/SP\",    \"partSearchOptions\": \"None\"  }
url = "https://api.mouser.com/api/v1/search/partnumber?apiKey=75cebc2d-2072-4155-910e-76d161912c9e"
headers = {
	"Accept"      : "application/json",
	"Content-Type": "application/json"
	}

data = {
	"SearchByPartRequest":
	{
		"mouserPartNumber": "",
		"partSearchOptions": None
		}
	}


import os
import digikey
from digikey.v3.productinformation import KeywordSearchRequest

os.environ['DIGIKEY_CLIENT_ID'] = 'scgqnnHtAKHCAfKhc7aNBwl6zXj1PGpj'
os.environ['DIGIKEY_CLIENT_SECRET'] = 'cF4RIemmluVMML7A'
os.environ['DIGIKEY_CLIENT_SANDBOX'] = 'False'
os.environ['DIGIKEY_STORAGE_PATH'] = 'digikey'

from time import sleep
#from serial import Serial
#scanner = Serial('COM3', 9600, timeout=0.1) # barcode scanner
supplier = "Mouser"



def draw_boxes(image, bounds, color='yellow', width=2):
    draw = ImageDraw.Draw(image)
    for bound in bounds:
        p0, p1, p2, p3 = bound[0]
        draw.line([*p0, *p1, *p2, *p3, *p0], fill=color, width=width)
    return image

def return_text(bounds):
  full_text = ""
  for bound in bounds: 
    text = bound[1]
    full_text = full_text + text
  return full_text

def load_img(img_path):
	img_abgr = cv2.imread(img_path)
	return cv2.cvtColor(img_abgr, cv2.COLOR_BGR2GRAY)

def show_img(img_obj):
	plt.imshow(img_obj, vmin=0, vmax=255)
	plt.axis("off")
	plt.show()

# def detect_boxes(cropped, height):
# 	pytesseract.pytesseract.tesseract_cmd = 'C:\\Program Files\\Tesseract-OCR\\tesseract'
# 	d = pytesseract.image_to_boxes(cropped, output_type=Output.DICT)
# 	print(d)

# 	if (len(d) > 0):
# 		num_boxes = len(d['char'])
# 		for i in range(num_boxes):
# 			(text,x1,y2,x2,y1) = (d['char'][i], d['left'][i], d['top'][i], d['right'][i], d['bottom'][i])
# 			cv2.rectangle(cropped, (x1, height-y1), (x2, height-y2), (0,255,0))
		
# 	return cropped

def start_webcam():
	cam = cv2.VideoCapture(1)
	cv2.namedWindow("Webcam")
	while True:
		ret, frame_bgr = cam.read()
		if ret:
			cv2.imshow('Webcam', frame_bgr)
			key = cv2.waitKey(100)
			if key == 32:
				chars = []
				print("Space bar pressed.")
				cv2.imwrite('frame.jpg', frame_bgr)
				captured = cv2.imread('frame.jpg', cv2.IMREAD_COLOR)
				cv2.imshow('Select ROI and press enter', captured)

				# Select ROI and crop
				r = cv2.selectROI("Select ROI and press enter", captured)
				cropped = captured[int(r[1]):int(r[1]+r[3]),
									int(r[0]):int(r[0]+r[2])]
				cv2.destroyWindow("Select ROI and press enter")
				cv2.imshow('ROI', cropped)
				cv2.imwrite('cropped.jpg', cropped)
				
				return(cropped)
			
			elif key == ord('q'):
				break


		else:
			cam.release()
			cam = None
			print("failed to grab frame")
			sleep(10)
			cam = cv2.VideoCapture(0, cv2.CAP_DSHOW)
			cam.set(cv2.CAP_PROP_FRAME_WIDTH, 800)
			cam.set(cv2.CAP_PROP_FRAME_HEIGHT,600)
			sleep(1)

# use easyOCR 
def ocr():
	reader = easyocr.Reader(['en'], model_storage_directory = "C:/Users/james/.EasyOCR/model/", download_enabled = False)
	bounds = reader.readtext('cropped.jpg')
	text = return_text(bounds)
	return(text)

# pytesseract.pytesseract.tesseract_cmd = 'C:\\Program Files\\Tesseract-OCR\\tesseract'
	

while True:
	try:
		f = open("c:/Users/james/source/repos/InventorySystem/mouser_digikey_part.txt", 'r+')
		txt = f.read()
		f.seek(0)
		f.write('')
		f.truncate()
		f.close()
		print(txt)
	except:
		print("no")
		continue
	
	if len(txt) > 2:
		
		dic = txt.split(',')
		partnumber = dic[1].strip()
		supplier = dic[0].strip()
		
		print(partnumber, supplier)
		
		# Mouser and Digi-Key
		# dic = partnumber.split("-ND") # +chr(29)
		# if len(dic) == 2:
		# 	dic = dic[0].split(chr(29))
			
		# 	if dic[1][0] == "P":
		# 		#print(dic[1][1:] + "-ND")
		# 		partnumber = dic[1][1:] + "-ND"
		# 		print(partnumber)
		# 		supplier = "Digi-Key"
		# 	else:
		# 		print("but cannot get the part number.")
		# 		continue
		
		print(supplier)
		description = []
		
		# Digi-Key search
		if supplier == "Digi-Key":
			print("This is DigiKey barcode.")
			digipart = digikey.product_details(partnumber)
			description.append(digipart.product_description)
			print(digipart)
			print()
		
		# Mouser and other suppliers search
		else:
			data["SearchByPartRequest"]["mouserPartNumber"] = partnumber
		
			try:
				response = requests.post(url, headers=headers, json=data)
			except:
				print(format_exc())
				break
			
			if response.status_code == 200:
				print(dumps(response.json(), indent=4))
				
				if "Errors" in response.json():
					Errors = response.json()["Errors"]
					if len(Errors) > 0:
						print(dumps(Errors, indent=4))
						continue # discard the results
				
				if "SearchResults" in response.json():
					SearchResults = response.json()["SearchResults"]
					
					if SearchResults["NumberOfResult"] > 0:
						print("There are", SearchResults["NumberOfResult"], "results.")
						for itm in SearchResults["Parts"]:
							description.append(itm["Description"])
					
					# Redirect to OCR if description cannot be found thru Mouser's API.
					else:
						print("Search returned no results. Use OCR.")
						cropped = start_webcam()
						height = cropped.shape[0]
						# bounded = detect_boxes(cropped, height)
						# show_img(bounded)
						# print("Webcam and bounding boxes successful")
						text = ocr()
						description.append(text)
						cv2.destroyAllWindows()
		
		print("The description for", partnumber)
		for desc in description:
			print(desc)

		try:
			desc = open("c:/Users/james/source/repos/InventorySystem/description.txt", 'r+')
			desc.seek(0)
			for descrip in description: 
				desc.write(descrip + '\n')
			desc.truncate()
			desc.close()
		except:
			print("Did not write description to file successfully.")
	sleep(2)


		
	
