# important variables
modname = Commands
version = $(shell cat modInfo.json | awk '/"version"/ {print $$3}' | sed 's/[",]//g')
zip_files_extra = "announcements.example.json" "protection-ranges.example.json" "chatcolors.example.json"
build_dir = "adrenalynn/$(moddir)"
fullname = Colony$(modname)Mod
moddir = $(fullname)
zipname = $(fullname)-$(version).zip
dllname = $(modname).dll

#
# actual build targets
#

default:
	mcs /target:library -r:../../../colonyserver_Data/Managed/Assembly-CSharp.dll,../Pipliz/APIProvider/APIProvider.dll,../../../colonyserver_Data/Managed/UnityEngine.dll -out:"$(dllname)" -sdk:2 src/*.cs

clean:
	rm -f "$(dllname)"
	[ -d "$(build_dir)" ] && rm -rf "$(build_dir)"

all: clean default zip

zip: default
	rm -f "$(zipname)"
	mkdir -p $(build_dir)
	cp modInfo.json "$(dllname)" $(zip_files_extra) $(build_dir)/
	zip -r "$(zipname)" $(build_dir)
	rm -r $(build_dir)
