#!/usr/bin/bash

function ask_yes_or_no() {
    read -p "$1 ([y]es or [N]o): "
    case $(echo $REPLY | tr '[A-Z]' '[a-z]') in
        y|yes) echo "yes" ;;
        *)     echo "no" ;;
    esac
}

file=ASPTextWriter-${1}.zip

if [[ "no" == $(ask_yes_or_no "Create release $file") ]]
then
    exit 0;
fi

echo creating $file

cd /cygdrive/d/ksp/asp/ASPTextWriter
rm -f Ignore/README-ASP-TextWriter.txt
rm -f Ignore/INSTALL-ASP-TextWriter.txt
rm -f Ignore/$file

cp README.md Ignore/README-ASP-TextWriter.txt
cp INSTALL.txt Ignore/INSTALL-ASP-TextWriter.txt

cd Ignore
zip $file README-ASP-TextWriter.txt
zip $file INSTALL-ASP-TextWriter.txt

cd /cygdrive/d/ksp/asp/ksptest/GameData
zip -r /cygdrive/d/ksp/asp/ASPTextWriter/Ignore/$file ASP/ASPTextWriter
