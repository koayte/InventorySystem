import requests
from json import dumps
from traceback import format_exc
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

while True:
	sleep(1)
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
		
		dic = partnumber.split("-ND") # +chr(29)
		if len(dic) == 2:
			print("This should be DigiKey barcode.")
			dic = dic[0].split(chr(29))
			
			if len(dic) == 2:
				if dic[1][0] == "P":
					#print(dic[1][1:] + "-ND")
					partnumber = dic[1][1:] + "-ND"
					print(partnumber)
					supplier = "Digi-Key"
				else:
					print("but cannot get the part number.")
					continue
		
		print(supplier)
		description = []
		
		if supplier == "Digi-Key":
			digipart = digikey.product_details(partnumber)
			description.append(digipart.product_description)
			print(digipart)
			print()
		
		elif supplier == "Mouser":
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
					else:
						print("The search returned no results.")
		else:
			print("Use OCR")
		
		print("The description for",partnumber)
		print(dumps(description, indent=4))

		try:
			desc = open("c:/Users/james/source/repos/InventorySystem/description.txt", 'r+')
			desc.seek(0)
			desc.write(dumps(description))
			desc.truncate()
			desc.close()
		except:
			print("did not write description")
	
