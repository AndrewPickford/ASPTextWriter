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
mkdir -p Ignore/GameData/ASP/TheWriteStuff
rm -f Ignore/GameData/ASP/TheWriteStuff/the-write-stuff.cfg

cp README.md Ignore/README-The-Write-Stuff.txt
cp INSTALL.txt Ignore/INSTALL-The-Write-Stuff.txt
cat GameData/ASP/TheWriteStuff/the-write-stuff.cfg | sed -e 's/debugLevel.*/debugLevel = 1/' > Ignore/GameData/ASP/TheWriteStuff/the-write-stuff.cfg

cd Ignore
zip $file README-The-Write-Stuff.txt
zip $file INSTALL-The-Write-Stuff.txt
zip $file GameData/ASP/TheWriteStuff/the-write-stuff.cfg

cd /cygdrive/d/ksp/asp/ASP-TheWriteStuff/GameData

find . -perm 000 -exec chmod 644 {} \;
find . -type d ! -perm -u+x -exec chmod a+x {} \;

zip /cygdrive/d/ksp/asp/ASP-TheWriteStuff/Ignore/$file -r --exclude ASP/TheWriteStuff/the-write-stuff.cfg ASP/TheWriteStuff
