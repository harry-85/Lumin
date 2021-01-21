#!/bin/sh

hashOutput=$(hash dotnet)
netCoreDownloadFileName="aspnetcore-runtime-5.0.0-linux-arm64.tar.gz"

dotnetPath="/usr/share/dotnet"

discoveryPort=8888
signalPort=5000

#install dotNet 5.0
if [[ $hashOutput != "" ]]
then
	wget https://download.visualstudio.microsoft.com/download/pr/ac555882-afa3-4f5b-842b-c4cec2ae0e90/84cdd6d47a9f79b6722f0e0a9b258888/aspnetcore-runtime-5.0.0-linux-arm64.tar.gz
	mkdir $dotnetPath
	export PATH=$PATH:$dotnetPath
	tar zxf $netCoreDownloadFileName -C $dotnetPath
	rm $netCoreDownloadFileName
else
	dotnetPath=$(whereis dotnet)
fi

echo dotNet path is at: $dotnetPath 

#open ports in firewall
ufw allow $discoveryPort
ufw allow $signalPort

#write config file to: ~/.luminConfig/lumin.config

#CONFIG FILE Start
#-------------------------------------------
	#Name of the Led Client
	Name=Bed Room
	#Number of LEDs at the LedClient side
	LedCount=58
	#Time in hours for auto off timer
	AutoOffTime=2
	#Discovery Port 
	DiscoveryPort=8080
#-------------------------------------------
#CONFIG FILE End

#write GPIO and SPI files, add users to enable SPI and GPIO | Path: /etc/udev/rules.d/

#install "libgpiod" --> see here: https://ubuntu.pkgs.org/20.04/ubuntu-universe-amd64/libgpiod-dev_1.4.1-4_amd64.deb.html
apt install libgpiod2 -y

# Create Service File / Enable and Start Service (systemctl)

# possibility 1:
echo "line 1" >> greetings.txt
echo "line 2" >> greetings.txt