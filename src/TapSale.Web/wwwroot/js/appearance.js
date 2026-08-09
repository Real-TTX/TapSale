(() => {
  const form = document.getElementById('appearanceForm');
  if (!form) return;

  const root = document.documentElement;
  const themeInputs = [...form.querySelectorAll('input[name="Input.Theme"]')];
  const colorInputs = [...form.querySelectorAll('[data-theme-color]')];
  const colorTextInputs = [...form.querySelectorAll('[data-color-text]')];
  const customColors = document.getElementById('customColors');
  const customChoice = document.getElementById('customThemeChoice');
  const resetButton = document.getElementById('resetThemeColors');
  const defaults = {
    '--ink': '#102a2a',
    '--brand': '#167d6d',
    '--lime': '#e4f26b',
    '--paper': '#f4f6f1',
    '--danger': '#bc3c3c'
  };

  const selectedTheme = () => themeInputs.find(input => input.checked)?.value ?? 'classic';
  const textInputFor = color => colorTextInputs.find(input => input.dataset.colorText === color.dataset.themeColor);
  const applyCustomColors = () => {
    for (const input of colorInputs) root.style.setProperty(input.dataset.themeColor, input.value);
  };
  const clearCustomColors = () => {
    for (const input of colorInputs) root.style.removeProperty(input.dataset.themeColor);
  };
  const refresh = () => {
    const theme = selectedTheme();
    const custom = theme === 'custom';
    root.dataset.theme = theme;
    customColors.hidden = !custom;
    if (custom) applyCustomColors(); else clearCustomColors();
  };

  for (const input of themeInputs) input.addEventListener('change', refresh);
  for (const input of colorInputs) input.addEventListener('input', () => {
    const textInput = textInputFor(input);
    if (textInput) {
      textInput.value = input.value;
      textInput.classList.remove('invalid');
    }
    if (selectedTheme() === 'custom') applyCustomColors();
  });
  for (const textInput of colorTextInputs) textInput.addEventListener('input', () => {
    const valid = /^#[0-9a-f]{6}$/i.test(textInput.value);
    textInput.classList.toggle('invalid', !valid);
    textInput.setCustomValidity(valid ? '' : form.dataset.invalidColor);
    if (!valid) return;
    const colorInput = colorInputs.find(input => input.dataset.themeColor === textInput.dataset.colorText);
    if (colorInput) colorInput.value = textInput.value;
    if (selectedTheme() === 'custom') applyCustomColors();
  });
  resetButton?.addEventListener('click', () => {
    for (const input of colorInputs) {
      input.value = defaults[input.dataset.themeColor];
      const textInput = textInputFor(input);
      if (textInput) {
        textInput.value = input.value;
        textInput.classList.remove('invalid');
      }
    }
    const customInput = themeInputs.find(input => input.value === 'custom');
    if (customInput) customInput.checked = true;
    customChoice?.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    refresh();
  });

  refresh();
})();
