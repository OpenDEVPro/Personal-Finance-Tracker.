async function fetchJSON(url, opts) {
  const res = await fetch(url, opts);
  if (!res.ok) throw new Error(res.statusText);
  return res.json();
}

async function loadCategories() {
  const cats = await fetchJSON('/api/categories');
  const sel = document.getElementById('category');
  sel.innerHTML = '';
  cats.forEach(c => {
    const o = document.createElement('option'); o.value = c; o.textContent = c; sel.appendChild(o);
  });
}

async function loadTransactions() {
  const rows = await fetchJSON('/api/transactions');
  const tbody = document.querySelector('#txTable tbody');
  tbody.innerHTML = '';
  rows.forEach(r => {
    const tr = document.createElement('tr');
    const date = new Date(r.date).toISOString().split('T')[0];
    tr.innerHTML = `<td>${date}</td><td>${Number(r.amount).toFixed(2)}</td><td>${r.category}</td><td>${r.note || ''}</td>`;
    const actions = document.createElement('td');
    const del = document.createElement('button'); del.textContent = 'Delete';
    del.addEventListener('click', async () => {
      if (!confirm('Delete this transaction?')) return;
      await fetch(`/api/transactions/${r.id}`, { method: 'DELETE' });
      loadTransactions(); loadSummary();
    });
    actions.appendChild(del);
    tr.appendChild(actions);
    tbody.appendChild(tr);
  });
}

async function loadSummary(){
  try{
    const s = await fetchJSON('/api/summary');
    document.getElementById('balance').textContent = Number(s.balance).toFixed(2);
    document.getElementById('income').textContent = Number(s.income).toFixed(2);
    document.getElementById('expenses').textContent = Number(s.expenses).toFixed(2);
  }catch(e){console.warn('Summary load failed', e)}
}

document.getElementById('txForm').addEventListener('submit', async e => {
  e.preventDefault();
  const data = {
    date: document.getElementById('date').value || null,
    amount: Number(document.getElementById('amount').value || 0),
    category: document.getElementById('category').value,
    note: document.getElementById('note').value
  };
  try {
    await fetchJSON('/api/transactions', {method: 'POST', headers: {'Content-Type':'application/json'}, body: JSON.stringify(data)});
    document.getElementById('txForm').reset();
    loadTransactions(); loadSummary();
  } catch (err) { alert('Error adding transaction: '+err.message); }
});

window.addEventListener('load', () => { loadCategories(); loadTransactions(); });

document.getElementById('exportBtn').addEventListener('click', () => {
  window.location.href = '/api/transactions/export';
});

window.addEventListener('load', () => { loadSummary(); });
