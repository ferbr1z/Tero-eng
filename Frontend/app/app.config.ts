export default defineAppConfig({
  ui: {
    colors: {
      primary: 'green',
      neutral: 'zinc'
    },
    button: { defaultVariants: { size: 'lg' } },
    input: { defaultVariants: { size: 'lg' } },
    inputNumber: { defaultVariants: { size: 'lg' } },
    textarea: { defaultVariants: { size: 'lg' } },
    select: {
      defaultVariants: { size: 'lg' },
      slots: { base: 'min-w-0', value: 'truncate', itemLabel: 'whitespace-normal' }
    },
    selectMenu: {
      defaultVariants: { size: 'lg' },
      slots: { base: 'min-w-0', value: 'truncate', itemLabel: 'whitespace-normal' }
    },
    checkbox: { defaultVariants: { size: 'lg' } },
    checkboxGroup: { defaultVariants: { size: 'lg' } },
    switch: { defaultVariants: { size: 'lg' } },
    dropdownMenu: { defaultVariants: { size: 'lg' } },
    tabs: { defaultVariants: { size: 'lg' } },
    navigationMenu: {
      slots: {
        link: 'px-3 py-2.5 text-sm',
        linkLeadingIcon: 'size-6',
        linkTrailingIcon: 'size-6',
        childLink: 'px-3 py-2.5 text-sm',
        childLinkIcon: 'size-6'
      }
    }
  }
})
