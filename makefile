# important variables
modname = Commands
fullname = Colony$(modname)Mod
zipname = $(fullname)-$(version).zip
dllname = $(modname).dll
version = $(shell cat modInfo.json | awk '/"version"/ {print $$3}' | head -1 | sed 's/[",]//g')
zip_files_extra = "announcements.example.json" "protection-ranges.example.json" "chatcolors.example.json"
build_dir = "adrenalynn/$(fullname)"
gamedir = "/local/games/Steam/steamapps/common/Colony Survival"

$(dllname): src/*.cs
	mcs /target:library -r:$(gamedir)/colonyserver_Data/Managed/Assembly-CSharp.dll,$(gamedir)/gamedata/mods/Pipliz/APIProvider/APIProvider.dll,$(gamedir)/colonyserver_Data/Managed/UnityEngine.dll -out:"$(dllname)" -sdk:2 src/*.cs

$(zipname): $(dllname)
	rm $(zipname)
	mkdir -p $(build_dir)
	cp modInfo.json LICENSE README.md $(dllname) $(zip_files_extra) $(build_dir)/
	zip -r $(zipname) $(build_dir)
	rm -r $(build_dir)

.PHONY: build default clean all zip install
build: $(dllname)

default: build

clean:
	-rm $(dllname) $(zipname)

all: clean default zip

zip: $(zipname)

install: build zip
	rm -r $(gamedir)/gamedata/mods/$(build_dir)
	unzip $(zipname) -d $(gamedir)/gamedata/mods

