#!/usr/bin/env python3
"""
Convert a txt file of joint angles (hip,knee,ankle per line, as used by RandomAngles.ino)
into a CSV that RobotMovement.cs can read to drive the simulated robot with the same
sequence of left-leg poses.

- Arduino (RandomAngles.ino): receives lines over serial, each line = "hip,knee,ankle";
  applies to left leg only (hipL, kneeL, ankleL).
- RobotMovement.cs: expects CSV with header row of column names. Columns may be:
  - Joint names (e.g. leftFemur, leftTibia, leftFoot): value = target angle in degrees.
  - Optional "wait" column: value = wait time in milliseconds (parsed and used as
    WaitForSeconds(value / 1000) before the next column is processed).
  Each data row gives values for those columns (floats).

Mapping: hip -> leftFemur, knee -> leftTibia, ankle -> leftFoot.
"""

import argparse
import csv
import sys
from pathlib import Path


# Joint limits from RandomAngles.ino (for validation/clamping if desired)
HIP_MIN, HIP_MAX = 0, 180
KNEE_MIN, KNEE_MAX = 0, 150
ANKLE_MIN, ANKLE_MAX = 0, 120

JOINT_HEADERS = ("leftFemur", "leftTibia", "leftFoot")
# Optional "wait" column: value in milliseconds (RobotMovement.cs parses it and waits value/1000 seconds)
WAIT_HEADER = "wait"


def clamp_angles(hip: int, knee: int, ankle: int) -> tuple[int, int, int]:
    """Clamp to Arduino joint limits (same as RandomAngles.ino)."""
    hip = max(HIP_MIN, min(HIP_MAX, hip))
    knee = max(KNEE_MIN, min(KNEE_MAX, knee))
    ankle = max(ANKLE_MIN, min(ANKLE_MAX, ankle))
    return hip, knee, ankle


def parse_txt_line(line: str) -> tuple[int, int, int] | None:
    """Parse one line 'hip,knee,ankle' into (hip, knee, ankle). Returns None if invalid."""
    line = line.strip()
    if not line or line.startswith("#"):
        return None
    parts = line.split(",")
    if len(parts) != 3:
        return None
    try:
        hip, knee, ankle = int(parts[0].strip()), int(parts[1].strip()), int(parts[2].strip())
        return clamp_angles(hip, knee, ankle)
    except ValueError:
        return None


def read_angle_txt(path: Path) -> list[tuple[int, int, int]]:
    """Read txt file; return list of (hip, knee, ankle) tuples."""
    rows = []
    with open(path, "r", encoding="utf-8") as f:
        for i, line in enumerate(f, 1):
            parsed = parse_txt_line(line)
            if parsed is not None:
                rows.append(parsed)
            elif line.strip():
                print(f"Warning: skipping invalid line {i}: {line.strip()!r}", file=sys.stderr)
    return rows




def build_csv_headers(include_wait: bool, wait_first: bool) -> tuple[str, ...]:
    """Build header row. If include_wait: add 'wait' column (first or last). Value in milliseconds."""
    if not include_wait:
        return JOINT_HEADERS
    if wait_first:
        return (WAIT_HEADER,) + JOINT_HEADERS
    return JOINT_HEADERS + (WAIT_HEADER,)


def write_robot_movement_csv(
    rows: list[tuple[int, int, int]],
    out_path: Path,
    wait_ms: float | None = None,
    wait_first_column: bool = True,
) -> None:
    """Write CSV for RobotMovement.cs.

    Headers: leftFemur, leftTibia, leftFoot; optionally 'wait' (value in milliseconds).
    If wait_ms is set: add a 'wait' column. First data row gets 0, subsequent rows get wait_ms
    (to approximate delay between poses, e.g. Arduino's 2500 ms between lines).
    """
    include_wait = wait_ms is not None
    headers = build_csv_headers(include_wait, wait_first_column)
    with open(out_path, "w", newline="", encoding="utf-8") as f:
        w = csv.writer(f)
        w.writerow(headers)
        for i, (hip, knee, ankle) in enumerate(rows):
            row: list[int | float] = [hip, knee, ankle]
            if include_wait:
                ms = 0 if i == 0 else int(wait_ms)
                if wait_first_column:
                    row.insert(0, ms)
                else:
                    row.append(ms)
            w.writerow(row)


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Convert hip,knee,ankle txt (RandomAngles.ino format) to RobotMovement.cs CSV."
    )
    parser.add_argument(
        "input_txt",
        type=Path,
        help="Input txt file: one line per pose, format 'hip,knee,ankle'.",
    )
    parser.add_argument(
        "-o", "--output",
        type=Path,
        default=None,
        help="Output CSV path. Default: input name with .csv extension.",
    )
    parser.add_argument(
        "--wait-ms",
        type=float,
        default=None,
        metavar="MS",
        help="Add optional 'wait' column (RobotMovement.cs: value in milliseconds). "
        "First row gets 0, subsequent rows get MS. E.g. 2500 to approximate Arduino delay between lines.",
    )
    parser.add_argument(
        "--wait-column-first",
        action="store_true",
        help="If --wait-ms is set, put 'wait' column first instead of last.",
    )
    args = parser.parse_args()

    if not args.input_txt.exists():
        print(f"Error: input file not found: {args.input_txt}", file=sys.stderr)
        sys.exit(1)

    rows = read_angle_txt(args.input_txt)
    if not rows:
        print("Error: no valid hip,knee,ankle lines found.", file=sys.stderr)
        sys.exit(1)

    out_path = args.output or args.input_txt.with_suffix(".csv")
    write_robot_movement_csv(
        rows,
        out_path,
        wait_ms=args.wait_ms,
        wait_first_column=args.wait_column_first,
    )
    print(f"Wrote {len(rows)} poses to {out_path}")


if __name__ == "__main__":
    main()
