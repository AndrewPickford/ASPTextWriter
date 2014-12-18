#!/usr/bin/bash

function ask_yes_or_no() {
    read -p "$1 ([y]es or [N]o): "
    case $(echo $REPLY | tr '[A-Z]' '[a-z]') in
        y|yes) echo "yes" ;;
        *)     echo "no" ;;
    esac
}

file=TheWriteStuff-${1}.zip

if [[ "no" == $(ask_yes_or_no "Create release $file") ]]
then
    exit 0;
fi

echo creating $file

cd /cygdrive/d/ksp/asp/ASP-TheWriteStuff
rm -f Ignore/README-The-Write-Stuff.txt
rm -f Ignore/INSTALL-The-Write-Stuff.txt
rm -f Ignore/$file
mkdir -p Ignore/ASP/TheWriteStuff
rm -f Ignore/ASP/TheWriteStuff/TheWriteStuff.cfg

cp README.md Ignore/README-The-Write-Stuff.txt
cp INSTALL.txt Ignore/INSTALL-The-Write-Stuff.txt
cat GameData/ASP/TheWriteStuff/TheWriteStuff.cfg | sed -e 's/debugLevel.*/debugLevel = 1/' > Ignore/ASP/TheWriteStuff/TheWriteStuff.cfg
unix2dos Ignore/ASP/TheWriteStuff/TheWriteStuff.cfg 

cd Ignore
zip $file README-The-Write-Stuff.txt
zip $file INSTALL-The-Write-Stuff.txt
zip $file ASP/TheWriteStuff/TheWriteStuff.cfg

cd /cygdrive/d/ksp/asp/ASP-TheWriteStuff/GameData

find . -perm 000 -exec chmod 644 {} \;
find . -type d ! -perm -u+x -exec chmod a+x {} \;

zip -r /cygdrive/d/ksp/asp/ASP-TheWriteStuff/Ignore/$file ASP/TheWriteStuff --exclude ASP/TheWriteStuff/TheWriteStuff.cfg --exclude "ASP/TheWriteStuff/TWS Configs/other.cfg"
