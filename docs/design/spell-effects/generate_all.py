"""원소 스크립트 5개로 시트 20장을 그리고 Unity 에셋을 다시 연결한다."""
import runpy
from pathlib import Path

HERE = Path(__file__).resolve().parent

for script in ["fire.py", "ice.py", "rock.py", "lightning.py", "holy.py", "build_unity_assets.py"]:
    print(f"== {script}")
    runpy.run_path(str(HERE / script), run_name="__main__")
