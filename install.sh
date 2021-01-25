#!/bin/sh

#Write Service File Function
#Parameters: 1. servicePath 2. installPath 3. dotnetPath 4. DllName 5. userName
WriteServiceFile () {
	servicePath=$1
	installPath=$2
	dotnetPath=$3
	DllName=$4
	userName=$5

	# Create Service File / Enable and Start Service (systemctl)
	echo '[Unit]' > $servicePath
	echo 'Description=Lumin Service in .NET' >> $servicePath
	echo ''
	echo '# Location:'$servicePath >> $servicePath
	echo ''
	echo '[Service]' >> $servicePath
	echo 'Type=simple' >> $servicePath
	echo 'WorkingDirectory='$installPath >> $servicePath
	echo 'ExecStart='$dotnetPath'/dotnet '$installPath$DllName >> $servicePath
	echo 'User='$userName >> $servicePath
	echo ''
	echo '[Install]' >> $servicePath
	echo 'WantedBy=multi-user.target' >> $servicePath
}

##Main Program###

#Default Values
clientName="Bed Room"
ledCount=58

releaseRepository="https://github.com/Richy1989/Lumin"
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

#Clone, publish and install binaries
#installPath="/home/ubuntu/tempInstall"
mkdir $installPath
mkdir ".lumin_temp"
cd ".lumin_temp"
git clone $releaseRepository
dotnet publish "Lumin/LuminServer/LuminServer.csproj"
actualFolder=pwd #$(pwd)
cd "Lumin/LuminServer/bin/Debug/net5.0/publish/"
cp -R * $installPath
cd $actualFolder
cd ".."
rm -r -f ".lumin_temp"

#Enable the Server Service
systemctl enable $serverServiceName
echo $serverServiceName' is enabled' 
echo 'Next Step: Execute: systemctl start '$serverServiceName