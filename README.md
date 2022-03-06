# Linker
 
A Z-Wave dashboard that let's you control and configure Z-Wave devices.
It has a configurable main screen that shows Z-Wave values directly and in a trend.
It is a Microsoft UWP app that uses openZWave, it's ZWave connection is made through a Aeotec Gen5 Z-Wave USB Z-Stick (other sticks should also work)

Its purpose is to control Z-wave devices and act as a data gateway. 
Values are stored in an SQLite database and when internet is available it sends to a receiver using web sockets.
The web socket’s part is working but not thoroughly tested, I stopped maintaining this project. Still, it works fine, nice and stable as a Z-wave controller.

I ran this on a Raspberry 3 with Windows 10 IOT core. As a UWP app it also runs perfectly on Windows 10 and 11. 
The application is touch screen enabled. I used a 7" 720p screen with a AliExpress housing. 



![Mainscreen](https://user-images.githubusercontent.com/19152655/156924719-27e2a681-f3fa-48e1-a4f7-259c288fbaf1.png)



When selecting an item their data is shown in a touch friendly trend viewer.
![Trend](https://user-images.githubusercontent.com/19152655/156925631-d614aaef-8e17-4aeb-aca5-3556327e44c1.png)




Adding items to the main screen is done through this screen, selecting a + sign brings a small config screen for naming and optional data manipulation like multiplication. The 3 lines open this config screen again.

![Values1](https://user-images.githubusercontent.com/19152655/156925653-a1ee843b-1d7c-4fc2-962d-4e7b8f653de8.png)




## Settings screen
![ZWave-Settings](https://user-images.githubusercontent.com/19152655/156925831-bd6046bb-7b93-4be9-aaa8-a4b051ffbea1.png)


All changes are stored in a XML file named IoConfig, you can find it in your documents folder after the first boot.
