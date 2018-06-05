import cv2
import numpy as np
from darkflow.net.build import TFNet
import matplotlib.pyplot as plt

#%config InlineBackend.figure_format = 'svg'

options = {
  'model': 'cfg/tiny-yolo-voc-1c.cfg',
  'load': 'bin/tiny-yolo-voc.weights',
  'threshold': 0.63,
  'gpu': 0
}

tfnet = TFNet(options)

img = cv2.imread('century.jpg')
result = tfnet.return_predict(img)

print(result)

tl = (result[0]['topleft']['x'], result[0]['topleft']['y'])
br = (result[0]['bottomright']['x'], result[0]['bottomright']['y'])
label = result[0]['label']

img = cv2.rectangle(img, tl, br, (0, 255, 0), 7)
img = cv2.putText(img, label, tl, cv2.FONT_HERSHEY_COMPLEX, 1, (0, 0, 0), 2)
plt.imshow(img)
plt.show()
