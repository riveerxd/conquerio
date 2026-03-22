import React, {createContext, useContext, useEffect, useState} from 'react';

export interface Keybinds {
  up: string;
  down: string;
  left: string;
  right: string;
  boost: string;
  shield: string;
}

export type MinimapPosition = 'top-right' | 'top-left' | 'bottom-right' | 'bottom-left';

export interface GameSettings {
  keybinds: Keybinds;
  showGrid: boolean;
  minimapSize: number;
  minimapPosition: MinimapPosition;
  colorblindMode: boolean;
}

const DEFAULT_SETTINGS: GameSettings = {
  keybinds: {
    up: 'w',
    down: 's',
    left: 'a',
    right: 'd',
    boost: ' ',
    shield: 'Shift',
  },
  showGrid: true,
  minimapSize: 150,
  minimapPosition: 'top-right',
  colorblindMode: false,
};

interface SettingsContextType {
  settings: GameSettings;
  updateSettings: (newSettings: Partial<Omit<GameSettings, 'keybinds'>> & { keybinds?: Partial<Keybinds> }) => void;
  resetSettings: () => void;
}

const SettingsContext = createContext<SettingsContextType | undefined>(undefined);

export const SettingsProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [settings, setSettings] = useState<GameSettings>(() => {
    const saved = localStorage.getItem('conquerio_settings');
    if (saved) {
      try {
        const parsed = JSON.parse(saved);
        return { 
          ...DEFAULT_SETTINGS, 
          ...parsed, 
          keybinds: { ...DEFAULT_SETTINGS.keybinds, ...parsed.keybinds } 
        };
      } catch (e) {
        console.error('Failed to parse settings', e);
      }
    }
    return DEFAULT_SETTINGS;
  });

  useEffect(() => {
    localStorage.setItem('conquerio_settings', JSON.stringify(settings));
  }, [settings]);

  const updateSettings = (newSettings: Partial<Omit<GameSettings, 'keybinds'>> & { keybinds?: Partial<Keybinds> }) => {
      setSettings((prev) => {
          const updated: GameSettings = {...prev, ...newSettings, keybinds: prev.keybinds};
          if (newSettings.keybinds) {
              updated.keybinds = {...prev.keybinds, ...newSettings.keybinds} as Keybinds;
          }
          return updated;
      });
  };

  const resetSettings = () => {
    setSettings(DEFAULT_SETTINGS);
  };

  return (
    <SettingsContext.Provider value={{ settings, updateSettings, resetSettings }}>
      {children}
    </SettingsContext.Provider>
  );
};

export const useSettings = () => {
  const context = useContext(SettingsContext);
  if (!context) {
    throw new Error('useSettings must be used within a SettingsProvider');
  }
  return context;
};
