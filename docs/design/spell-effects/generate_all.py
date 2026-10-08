"""generated/ 의 원소 atlas 5장을 시트 20장으로 바꾸고 Unity 에셋을 다시 연결한다."""
import runpy
from pathlib import Path

HERE = Path(__file__).resolve().parent

for script in ["import_generated.py", "build_unity_assets.py"]:
    print(f"== {script}")
    runpy.run_path(str(HERE / script), run_name="__main__")
