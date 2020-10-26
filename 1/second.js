const sharp = require("sharp");
const fs = require("fs");

const image = sharp('./assets/harrison-qi-PBFtL7_RFJk-unsplash.jpg')
const normalize = (pixel) => {
    return Math.floor(pixel/16) * 16;
}

image
    .greyscale()
    .toColourspace('b-w')
    .toFile('./assets/outputGrey.jpg', () => {
        console.log('Grey photo saved');
        // const gray = sharp('./assets/outputGrey.jpg');
        // gray
        //     .jpeg()
        //     .tile({skipBlanks: 16})
        //     .toFile('./assets/normalized16.jpg', () => {
        //         console.log('Normalized Photo saved');
        //     });

        fs.readFile('./assets/outputGrey.jpg', (err, data) => {
            //data.buffer = data.buffer.map((pixel) => normalize(pixel));
            const buf = new Buffer.from(data);
            const arr = new Uint16Array(buf.buffer);

            const newData = drawArray(data, 16);
            fs.writeFile('./assets/normalized16.jpg', data, () => {
                console.log('Normalized Photo saved')
            })
        });
    });

image 
    .metadata()
    .then((meta) => {
        console.log('number of pixels: ', meta.width*meta.height);
    })

function drawArray(arr, depth) {
        var offset, height, data, image;
      
        function conv(size) {
          return String.fromCharCode(size&0xff, (size>>8)&0xff, (size>>16)&0xff, (size>>24)&0xff);
        }
      
        offset = depth <= 8 ? 54 + Math.pow(2, depth)*4 : 54;
        height = Math.ceil(Math.sqrt(arr.length * 8/depth));
      
        //BMP Header
        data  = 'BM';                          // ID field
        data += conv(offset + arr.length);     // BMP size
        data += conv(0);                       // unused
        data += conv(offset);                  // pixel data offset
        
        //DIB Header
        data += conv(40);                      // DIB header length
        data += conv(height);                  // image height
        data += conv(height);                  // image width
        data += String.fromCharCode(1, 0);     // colour panes
        data += String.fromCharCode(depth, 0); // bits per pixel
        data += conv(0);                       // compression method
        data += conv(arr.length);              // size of the raw data
        data += conv(2835);                    // horizontal print resolution
        data += conv(2835);                    // vertical print resolution
        data += conv(0);                       // colour palette, 0 == 2^n
        data += conv(0);                       // important colours
        
        //Grayscale tables for bit depths <= 8
        if (depth <= 8) {
          data += conv(0);
          
          for (var s = Math.floor(255/(Math.pow(2, depth)-1)), i = s; i < 256; i += s)  {
            data += conv(i + i*256 + i*65536);
          }
        }
        
        //Pixel data
        data += String.fromCharCode.apply(String, arr);
      
        return data;
}