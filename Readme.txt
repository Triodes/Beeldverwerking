Aron List 3896536
Noeri Huisman 4155521

- Use the checkboxes to turn drawing options on and off.
- Use the dropdown menu to select the output image.
- Use the batch button to process several files at once. 
  The output will be stored in the output folder which is created in the same directory as the input images.

We have revised the functions for converting bitmaps to int arrays. It's not perfect and doesn't work on indexed images (gifs) but it's much, much faster compared to using getPixel and setPixel. You might want to look into this for next years framework.