import subprocess
from pathlib import Path


FPS = 7
CONTENT_SOURCE_PATH = "C:/Users/drakan/source/repos/RoguelikeFNA/RoguelikeFNA/ContentSource/Atlases"
CONTENT_PATH = "C:/Users/drakan/source/repos/RoguelikeFNA/RoguelikeFNA/Content/Atlases"
EXECUTABLE_PATH = "C:/Users/drakan/source/repos/RoguelikeFNA/Nez/Nez.SpriteAtlasPacker/PrebuiltExecutable/SpriteAtlasPacker.exe"


def generate_sprite_atlases():
    for directory in Path(CONTENT_SOURCE_PATH).iterdir():
        if directory.is_file():
            continue
        dir_name = directory.name.lower()
        out_directory = Path(CONTENT_PATH, dir_name)
        out_directory.mkdir(exist_ok=True, parents=True)
        img_path = str(Path(out_directory, f"{dir_name}.png").absolute())
        map_path = str(Path(out_directory, f"{dir_name}.atlas").absolute())
        directory = str(directory.absolute())
        subprocess.run([EXECUTABLE_PATH, f"-image:{img_path}", f"-map:{map_path}", directory])


if __name__ == "__main__":
    generate_sprite_atlases()