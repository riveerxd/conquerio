import React, {useEffect, useState} from 'react';
import {Keybinds, MinimapPosition, useSettings} from './SettingsContext';

interface Props {
  onBack: () => void;
}

export default function SettingsMenu({ onBack }: Props) {
  const { settings, updateSettings, resetSettings } = useSettings();
  const [capturing, setCapturing] = useState<keyof Keybinds | null>(null);
    const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!capturing) return;

    const onKeyDown = (e: KeyboardEvent) => {
      e.preventDefault();
      e.stopPropagation();
      
      // Cancel capture with Escape, don't allow binding Escape as an action
      if (e.key === 'Escape') {
        setCapturing(null);
          setError(null);
          return;
      }

        // Check for conflicts
        const conflict = Object.entries(settings.keybinds).find(([k, v]) => v === e.key && k !== capturing);
        if (conflict) {
            setError(`Key "${e.key === ' ' ? 'Space' : e.key}" is already bound to ${conflict[0]}`);
        return;
      }

        const newKeybinds = {[capturing]: e.key};
      updateSettings({ keybinds: newKeybinds });
      setCapturing(null);
        setError(null);
    };

    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, [capturing, settings.keybinds, updateSettings]);

  const renderKeybind = (key: keyof Keybinds, label: string) => (
    <div style={styles.settingRow} key={key}>
      <span style={styles.label}>{label}</span>
      <div style={styles.keybindContainer}>
        <button
          style={{ ...styles.keyBtn, ...(capturing === key ? styles.capturing : {}) }}
          onClick={() => {
              setCapturing(key);
              setError(null);
          }}
          disabled={capturing !== null && capturing !== key}
        >
          {capturing === key ? 'Press any key... (ESC to cancel)' : settings.keybinds[key] === ' ' ? 'Space' : settings.keybinds[key]}
        </button>
        {capturing === key && (
          <button
            style={styles.cancelBtn}
            onClick={() => {
                setCapturing(null);
                setError(null);
            }}
          >
            Cancel
          </button>
        )}
      </div>
    </div>
  );

  return (
    <div style={styles.overlay} onClick={onBack}>
      <div style={styles.box} onClick={(e) => e.stopPropagation()}>
        <h2 style={styles.title}>settings</h2>

          {error && <div style={styles.errorBanner}>{error}</div>}

        <div style={styles.section}>
          <h3 style={styles.sectionTitle}>controls</h3>
          {renderKeybind('up', 'Move Up')}
          {renderKeybind('down', 'Move Down')}
          {renderKeybind('left', 'Move Left')}
          {renderKeybind('right', 'Move Right')}
          {renderKeybind('boost', 'Boost Ability')}
          {renderKeybind('shield', 'Shield Ability')}
        </div>

        <div style={styles.section}>
          <h3 style={styles.sectionTitle}>graphics</h3>
          <div style={styles.settingRow}>
            <span style={styles.label}>Show Grid Lines</span>
            <input
              type="checkbox"
              checked={settings.showGrid}
              onChange={(e) => updateSettings({ showGrid: e.target.checked })}
              style={styles.checkbox}
            />
          </div>
          <div style={styles.settingRow}>
            <span style={styles.label}>Colorblind Mode</span>
            <input
              type="checkbox"
              checked={settings.colorblindMode}
              onChange={(e) => updateSettings({ colorblindMode: e.target.checked })}
              style={styles.checkbox}
            />
          </div>
          <div style={styles.settingRow}>
            <span style={styles.label}>Minimap Size</span>
            <input
              type="range"
              min="100"
              max="300"
              step="10"
              value={settings.minimapSize}
              onChange={(e) => updateSettings({ minimapSize: parseInt(e.target.value) })}
              style={styles.range}
            />
            <span style={styles.valueText}>{settings.minimapSize}px</span>
          </div>
          <div style={styles.settingRow}>
            <span style={styles.label}>Minimap Position</span>
            <select
              value={settings.minimapPosition}
              onChange={(e) => updateSettings({ minimapPosition: e.target.value as MinimapPosition })}
              style={styles.select}
            >
              <option value="top-right">Top Right</option>
              <option value="top-left">Top Left</option>
              <option value="bottom-right">Bottom Right</option>
              <option value="bottom-left">Bottom Left</option>
            </select>
          </div>
        </div>

        <div style={styles.footer}>
          <button onClick={resetSettings} style={{ ...styles.btn, ...styles.resetBtn }}>reset defaults</button>
          <button onClick={onBack} style={styles.btn}>back</button>
        </div>
      </div>
    </div>
  );
}

const styles: Record<string, React.CSSProperties> = {
  overlay: {
    position: "fixed",
    inset: 0,
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    background: "rgba(0,0,0,0.8)",
    zIndex: 20,
    backdropFilter: "blur(4px)",
  },
  box: {
    display: "flex",
    flexDirection: "column",
    width: "400px",
    maxHeight: "90vh",
    overflowY: "auto",
    padding: "32px",
    background: "#111",
    border: "2px solid #333",
    color: "#fff",
    fontFamily: "monospace",
  },
  title: {
    fontSize: 24,
    margin: "0 0 24px 0",
    textAlign: "center",
    letterSpacing: "2px",
    textTransform: "uppercase",
  },
  section: {
    marginBottom: "24px",
  },
    errorBanner: {
        background: "#ff4444",
        color: "#fff",
        padding: "8px",
        fontSize: "12px",
        marginBottom: "16px",
        textAlign: "center",
        borderRadius: "2px",
    },
  sectionTitle: {
    fontSize: 14,
    color: "#888",
    marginBottom: "12px",
    textTransform: "uppercase",
    borderBottom: "1px solid #333",
    paddingBottom: "4px",
  },
  settingRow: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: "8px",
  },
  label: {
    fontSize: 14,
  },
  keybindContainer: {
    display: "flex",
    gap: "8px",
    alignItems: "center",
  },
  keyBtn: {
    minWidth: "120px",
    padding: "6px 12px",
    background: "#222",
    border: "1px solid #444",
    color: "#fff",
    fontFamily: "monospace",
    cursor: "pointer",
    textAlign: "center",
  },
  capturing: {
    background: "#444",
    borderColor: "#fff",
  },
  cancelBtn: {
    padding: "6px 12px",
    background: "#555",
    border: "1px solid #777",
    color: "#fff",
    fontFamily: "monospace",
    cursor: "pointer",
    fontSize: 12,
    textTransform: "uppercase",
  },
  checkbox: {
    cursor: "pointer",
    width: "18px",
    height: "18px",
  },
  range: {
    flex: 1,
    margin: "0 12px",
  },
  valueText: {
    minWidth: "40px",
    textAlign: "right",
    fontSize: 12,
    color: "#aaa",
  },
  select: {
    padding: "4px 8px",
    background: "#222",
    color: "#fff",
    border: "1px solid #444",
    fontFamily: "monospace",
  },
  footer: {
    display: "flex",
    flexDirection: "column",
    gap: "12px",
    marginTop: "12px",
  },
  btn: {
    padding: "10px 0",
    background: "#fff",
    color: "#111",
    border: "none",
    fontFamily: "monospace",
    fontSize: 14,
    fontWeight: "bold",
    cursor: "pointer",
    textTransform: "uppercase",
  },
  resetBtn: {
    background: "none",
    border: "1px solid #555",
    color: "#aaa",
  },
};
