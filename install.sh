#!/bin/sh

hashOutput=$(hash dotnet)
netCoreDownloadFileName="aspnetcore-runtime-5.0.0-linux-arm64.tar.gz"

dotnetPath="/usr/share/dotnet"


if [[ $hashOutput != "" ]]
then
	wget https://download.visualstudio.microsoft.com/download/pr/ac555882-afa3-4f5b-842b-c4cec2ae0e90/84cdd6d47a9f79b6722f0e0a9b258888/aspnetcore-runtime-5.0.0-linux-arm64.tar.gz
	mkdir $dotnetPath
	export PATH=$PATH:$dotnetPath
	tar zxf $netCoreDownloadFileName -C $dotnetPath
	rm $netCoreDownloadFileName
else
	$dotnetPath=$(whereis dotnet)
fi

