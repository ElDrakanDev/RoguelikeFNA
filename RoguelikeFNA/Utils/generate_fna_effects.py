import shutil
import subprocess
from pathlib import Path
import os


CONTENT_SOURCE_PATH = "../ContentSource/Effects"
CONTENT_PATH = "../Content/Shaders"
EXECUTABLE_PATH = "../../Nez/DefaultContentSource/FNAShaderCompiler/fxc.exe"


def generate_fna_effects():
    for file in Path(CONTENT_SOURCE_PATH).iterdir():

        # We ignore non-effect files or directories
        if not file.is_file() or not file.suffix == ".fx":
            continue

        print(f"Compiling {file}")

        out_file = f"{file.stem}.fxb"
        in_file = str(file.absolute())
        subprocess.run([EXECUTABLE_PATH, "/Tfx_2_0", f"/Fo{out_file}", in_file])

        shutil.move(Path(f"./{out_file}").absolute(), Path(CONTENT_PATH, out_file).absolute())


if __name__ == "__main__":
    generate_fna_effects()