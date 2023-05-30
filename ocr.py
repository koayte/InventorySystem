import cv2
import easyocr
from time import sleep
from PIL import ImageDraw


#, cv2.CAP_DSHOW)
#cam.set(cv2.CAP_PROP_FRAME_WIDTH, 800)
#cam.set(cv2.CAP_PROP_FRAME_HEIGHT,600)



# Our ROI, defind by two points
p1, p2 = None, None
state = 0
wait = 0

# Called every time a mouse event happen
def on_mouse(event, x, y, flags, userdata):
    global state, p1, p2, wait
    
    # Left click
    if event == cv2.EVENT_LBUTTONUP:
        # Select first point
        if state == 0:
            p1 = (x,y)
            state += 1
        # Select second point
        elif state == 1:
            p2 = (x,y)
            state += 1
    # Right click (erase current ROI)
    if event == cv2.EVENT_RBUTTONUP:
        p1, p2 = None, None
        state = 0
        wait = 0
        cv2.destroyWindow('image')

        

##while True:
##    # Register the mouse callback
##    cv2.setMouseCallback('Webcam', on_mouse)
##    
##    ret, frame_bgr = cam.read()
##    if ret:
##        # img = cv2.imread(frame_bgr)    
##        
##        # If a ROI is selected, draw it
##        if state > 1:
##            wait += 1
##            cv2.rectangle(frame_bgr, p1, p2, (255, 0, 0), 1)
##            if wait == 1:
##                crop = frame_bgr[p1[1]:p2[1], p1[0]:p2[0]] # y:y+h, x:x+w
##                cv2.imwrite('frame.jpg', crop)
##                cropped = cv2.imread('frame.jpg', cv2.IMREAD_COLOR)
##                cv2.imshow('image', cropped)
##        cv2.imshow('Webcam', frame_bgr)
##        key = cv2.waitKey(10)
##        if key == ord('q'):
##            break
##        
##    else:
##        cam.release()
##        cam = None
##        print("failed to grab frame")
##        sleep(10)
##        cam = cv2.VideoCapture(0, cv2.CAP_DSHOW)
##        cam.set(cv2.CAP_PROP_FRAME_WIDTH, 800)
##        cam.set(cv2.CAP_PROP_FRAME_HEIGHT,600)
##        sleep(1)

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
    full_text = full_text + text + '\n'
  return full_text

while True:
    cam = cv2.VideoCapture(1)
    cv2.namedWindow("Webcam")
    while True:
        ret, frame_bgr = cam.read()
        if ret:
            key = cv2.waitKey(10)
            # Capture if space key is pressed
            if key == 32:
                cv2.imwrite('frame.jpg', frame_bgr)
                captured = cv2.imread('frame.jpg', cv2.IMREAD_COLOR)
                cv2.imshow('Select area and press enter', captured)

                # Select ROI and crop
                r = cv2.selectROI("Select area and press enter", captured)
                cropped = captured[int(r[1]):int(r[1]+r[3]),
                                    int(r[0]):int(r[0]+r[2])]
                cv2.imshow('Select area and press enter', cropped)
                cv2.imwrite('cropped.jpg', cropped)

                # OCR
                reader = easyocr.Reader(['en'], model_storage_directory = "C:/Users/james/.EasyOCR/model/", download_enabled = False)
                bounds = reader.readtext('cropped.jpg')
                full_text = return_text(bounds)
                print(full_text)
            
            elif key == ord('q'):
                break

            cv2.imshow('Webcam', frame_bgr)

            
        else:
            cam.release()
            cam = None
            print("failed to grab frame")
            sleep(10)
            cam = cv2.VideoCapture(0, cv2.CAP_DSHOW)
            cam.set(cv2.CAP_PROP_FRAME_WIDTH, 800)
            cam.set(cv2.CAP_PROP_FRAME_HEIGHT,600)
            sleep(1)

##As usual recognition algorithms are not invariant to rotation. And every image seems to be geometically distorted similarly. You can try to normalize the geometry by warpPerspective function from Opencv with appropriate transformation matrix. Rotation is a subset of all possible transformations covered by perspective transform.
##You can try to use advanced deblurring techniques like wiener filter or deeplearning. It seems like point spread function is different from image to image that complecates the recovery.
##There is some periodic signal in your images (vertical blue-white-blue stripes). That can possibly can be enhanced by doing FFT -> removing components of the specific wavelength -> iFFT.
