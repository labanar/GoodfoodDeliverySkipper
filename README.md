# GoodfoodDeliverySkipper
Automates the skipping of weekly Goodfood deliveries.



1) Using docker run<br/>
`docker run -e GOODFOODAUTH__USERNAME='YOUR_USERNAME' -e GOODFOODAUTH__PASSWORD='YOUR_PASSWORD' -e DISCORD__WEBHOOKURL='OPTIONAL' labanar/goodfoodskipper:latest`

2) Using docker-compose:
```
version: '3.4'

services:
  goodfoodskipper:
    image: labanar/goodfoodskipper:latest
    environment:
     - GOODFOODAUTH__USERNAME=YOUR_USERNAME
     - GOODFOODAUTH__PASSWORD=YOUR_PASSWORD
     - DISCORD__WEBHOOKURL=OPTIONAL
```
