#!/bin/sh

#Write Service File Function
#Parameters: 1. serverServicePath 2. installPath 3. dotnetPath 4. luminServerDllName 5. userName
WriteServiceFile () {
	serverServicePath=$1
	installPath=$2
	dotnetPath=$3
	luminServerDllName=$4
	userName=$5

	# Create Server Service File / Enable and Start Service (systemctl)
	echo '[Unit]' > $serverServicePath
	echo 'Description=Lumin Service in .NET' >> $serverServicePath
	echo ''
	echo '# Location:'$serverServicePath >> $serverServicePath
	echo ''
	echo '[Service]' >> $serverServicePath
	echo 'Type=simple' >> $serverServicePath
	echo 'WorkingDirectory='$installPath >> $serverServicePath
	echo 'ExecStart='$dotnetPath'/dotnet '$installPath$luminServerDllName >> $serverServicePath
	echo 'User='$userName >> $serverServicePath
	echo ''
	echo '[Install]' >> $serverServicePath
	echo 'WantedBy=multi-user.target' >> $serverServicePath
}

clientName="Bed Room"
ledCount=58

luminServerDllName="LuminServer.dll"

userName=$USER
homeFolder="/home/"$userName"/"

dotnetPath="/usr/share/dotnet"
installPath=$homeFolder"/lumin/"

luminConfigName="lumin.config"
luminConfigPath=$homeFolder".luminConfig/"$luminConfigName

deviceRulesPath="/etc/udev/rules.d/"
spiDeviceRule=$deviceRulesPath"50-spi.rules"

serverServiceName="LuminServerService.service"
serverServicePath="/etc/systemd/system/"$serverServiceName

discoveryPort=8080
signalPort=5000

hashOutput=$(hash dotnet)
netCoreDownloadFileName="aspnetcore-runtime-5.0.0-linux-arm64.tar.gz"

#check and install dotNet 5.0
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

#write config file to: luminConfigPath
echo '#CONFIG FILE Start' > $luminConfigPath
echo '#-------------------------------------------' >> $luminConfigPath
echo '#Name of the Led Client' >> $luminConfigPath
echo 'Name='$clientName >> $luminConfigPath
echo '#Number of LEDs at the LedClient side' >> $luminConfigPath
echo 'LedCount='$ledCount >> $luminConfigPath
echo '#Time in hours for auto off timer' >> $luminConfigPath
echo 'AutoOffTime=2' >> $luminConfigPath
echo '#Discovery Port' >> $luminConfigPath
echo 'DiscoveryPort='$discoveryPort >> $luminConfigPath
echo '#-------------------------------------------' >> $luminConfigPath
echo '#CONFIG FILE End' >> $luminConfigPath

#write GPIO and SPI files, add users to enable SPI and GPIO | Path: /etc/udev/rules.d/
groupadd spiuser
adduser "$USER" spiuser
echo 'SUBSYSTEM=="spidev", GROUP="spiuser", MODE="0660"' > $spiDeviceRule

#install "libgpiod" --> see here: https://ubuntu.pkgs.org/20.04/ubuntu-universe-amd64/libgpiod-dev_1.4.1-4_amd64.deb.html
apt install libgpiod2 -y

# Create Server Service File / Enable and Start Service (systemctl)
WriteServiceFile $serverServicePath $installPath $dotnetPath $luminServerDllName $userName

#Enable the Server Service
systemctl enable $serverServiceName
echo $serverServiceName' is enabled' 
echo '1. Copy Files to: '$installPath 
echo '2. Execute: systemctl start '$serverServiceName
