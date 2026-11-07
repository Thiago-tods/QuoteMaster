(() => {
  const list = document.getElementById('quotationItems');
  const template = document.getElementById('itemTemplate');
  const addButton = document.getElementById('addItem');
  const form = document.getElementById('quotationForm');
  if (!list || !template || !addButton || !form) return;

  const formatter = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });
  const parse = input => {
    const value = Number.parseFloat(input?.value ?? '0');
    return Number.isFinite(value) ? value : 0;
  };
  const roundMoney = value => Math.sign(value) * Math.floor(Math.abs(value) * 100 + 0.5) / 100;
  const updatePreview = () => {
    let subtotal = 0;
    list.querySelectorAll('[data-item-row]').forEach(row => {
      const quantity = parse(row.querySelector('.item-quantity'));
      const price = parse(row.querySelector('.item-price'));
      const discount = parse(row.querySelector('.item-discount'));
      subtotal += roundMoney(quantity * price - discount);
    });
    const total = Math.max(0, roundMoney(subtotal - parse(document.getElementById('generalDiscount'))));
    document.getElementById('previewSubtotal').textContent = formatter.format(subtotal);
    document.getElementById('previewTotal').textContent = formatter.format(total);
  };
  const reindex = () => {
    list.querySelectorAll('[data-item-row]').forEach((row, index) => {
      row.querySelectorAll('[name]').forEach(field => {
        field.name = field.name.replace(/Items\[\d+\]/, `Items[${index}]`);
        if (field.id) field.id = field.id.replace(/Items_\d+__/, `Items_${index}__`);
      });
    });
  };
  const bindRow = row => {
    row.querySelector('.catalog-select')?.addEventListener('change', event => {
      const selected = event.currentTarget.selectedOptions[0];
      const hasProduct = Boolean(selected?.value);
      const description = row.querySelector('.item-description');
      const price = row.querySelector('.item-price');
      if (hasProduct) {
        description.value = selected.dataset.name ?? '';
        price.value = selected.dataset.price ?? '';
      }
      description.readOnly = hasProduct;
      price.readOnly = hasProduct;
      updatePreview();
    });
    row.querySelectorAll('.numeric-preview').forEach(input => input.addEventListener('input', updatePreview));
    row.querySelector('[data-remove-item]')?.addEventListener('click', () => {
      row.remove();
      if (!list.querySelector('[data-item-row]')) addRow();
      reindex();
      updatePreview();
    });
    const selection = row.querySelector('.catalog-select');
    if (selection?.value) selection.dispatchEvent(new Event('change'));
  };
  const addRow = () => {
    const index = list.querySelectorAll('[data-item-row]').length;
    const html = template.innerHTML.replaceAll('__INDEX__', String(index));
    const fragment = document.createRange().createContextualFragment(html);
    const row = fragment.querySelector('[data-item-row]');
    list.append(fragment);
    if (row) bindRow(row);
  };

  addButton.addEventListener('click', () => { addRow(); reindex(); updatePreview(); });
  document.getElementById('generalDiscount')?.addEventListener('input', updatePreview);
  list.querySelectorAll('[data-item-row]').forEach(bindRow);
  updatePreview();
})();
