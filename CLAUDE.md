# The Film Archive - Conventions

## Frontend UI components

- Never ship a native browser form control (`<select>`, etc.) styled with CSS overrides as the fix for "it looks ugly." Build/use a real custom component instead.
- Dropdowns/selects: always use the shared `tfa-dropdown` component (`apps/client/src/components/dropdown/dropdown.component.ts`) instead of a native `<select>`. It takes `[options]` (`DropdownOption<T>[]` = `{ label, value }`), `[value]`, and emits `(valueChange)`. If it's missing something you need (e.g. `ControlValueAccessor`/`ngModel` support), extend that component rather than reaching for `<select>` again.
- This pattern generalizes: prefer a small number of shared, custom-styled components under `apps/client/src/components/` over ad-hoc native-element + CSS-override styling repeated per page.
