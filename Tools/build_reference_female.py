"""Build the separate adult female study with the shared geometry/rig pipeline."""
from pathlib import Path
import runpy
runpy.run_path(str(Path(__file__).with_name('build_reference_male_rig.py')), init_globals={'FEMALE': True})
