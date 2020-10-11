import skimage
import skimage.measure
from skimage import io

I = io.imread('assets\grey.bmp')
print('bmp: ' + str(skimage.measure.shannon_entropy(I)))

I = io.imread('assets\grey.jpg')
print('jpg: ' + str(skimage.measure.shannon_entropy(I)))

I = io.imread('assets\grey.tiff')
print('tiff: ' + str(skimage.measure.shannon_entropy(I)))

I = io.imread('assets\grey.png')
print('png: ' + str(skimage.measure.shannon_entropy(I)))