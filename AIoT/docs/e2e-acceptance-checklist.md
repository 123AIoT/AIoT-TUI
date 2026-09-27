# End-to-End Acceptance Checklist

This checklist is for quick regression in Visual Studio (TUI + CLI + CI-facing smoke script).

## 1) Build Baseline

- [ ] Open solution in Visual Studio 2026.
- [ ] Build solution (Debug/Release): no compile errors.
- [ ] Confirm app starts in TUI mode without arguments.

## 2) TUI – OPC UA Path

- [ ] Open **Connection > Connect...**
- [ ] Select protocol **OPC UA**.
- [ ] Connect to endpoint (example: `opc.tcp://localhost:4840`).
- [ ] Verify address space tree loads.
- [ ] Subscribe variable with `Enter`.
- [ ] Verify monitored table updates.
- [ ] Press `W` on writable variable and confirm write success.
- [ ] Open Scope (`S`) and confirm selected signals render.

## 3) TUI – Non-OPC Protocol Path (Byoe)

- [ ] Connect with one of: Modbus TCP / Modbus RTU / Siemens S7 / Mitsubishi MC / Omron FINS.
- [ ] Verify protocol hint panel appears (instead of OPC address tree).
- [ ] Press `A` to add monitor address.
- [ ] Press `E` to edit selected monitor address/type/length.
- [ ] Verify `string` / `hex` points reject empty length and invalid address formats are blocked.
- [ ] Verify value polling updates monitored table.
- [ ] Press `W` to write selected address.
- [ ] Press `Delete` to remove selected address.
- [ ] Trigger reconnect from menu and confirm non-OPC reconnect behavior.
- [ ] Save configuration and reload it; confirm protocol, endpoint, point type, and point length are restored.

## 4) Connect Dialog Parameter UX

- [ ] Siemens S7 shows dedicated `plc/rack/slot` fields.
- [ ] Modbus RTU shows dedicated `baud/station` fields.
- [ ] Invalid Siemens PLC type is rejected by validation.
- [ ] Invalid RTU baud is rejected by validation.

## 5) CLI Core Commands

- [ ] `read` works with `--format json`.
- [ ] `write` works with typed options (`--type bool|int|float|string|hex`).
- [ ] `monitor` works with `--count` + `--format ndjson`.
- [ ] Unsupported `--protocol` fails fast with clear error.

## 6) CLI Multi-Protocol Coverage

- [ ] OPC UA command path.
- [ ] Modbus TCP command path.
- [ ] Modbus RTU command path.
- [ ] Siemens S7 command path (`--plc/--rack/--slot` or query endpoint).
- [ ] Mitsubishi MC command path.
- [ ] Omron FINS command path.

## 7) Smoke Script + CI Alignment

- [ ] Run local smoke script: `./scripts/smoke-cli.ps1`.
- [ ] Run strict mode: `./scripts/smoke-cli.ps1 -Strict`.
- [ ] Verify skip switches behave as expected.
- [ ] Confirm GitHub workflow command matches intended CI scope (`.github/workflows/smoke-cli.yml`).

## 8) Documentation Consistency

- [ ] `README.md` reflects current protocol list and shortcuts.
- [ ] `README.md` reflects non-OPC point edit (`E`) and type/length persistence behavior.
- [ ] `docs/cli-smoke-tests.md` includes current command examples and options.
- [ ] Help text from `aiot --help` matches documented options.

---

## Exit Criteria

Release candidate is acceptable when:

- No compile errors,
- TUI flows pass for OPC + at least one non-OPC protocol,
- CLI typed read/write/monitor works,
- Smoke script runs clean for enabled protocol groups,
- Docs and `--help` stay consistent.
