// Firebase JS SDK should already be included in your HTML
const dbRef = firebase.database().ref('pangolins');
const container = document.getElementById('dashboard-container'); // your original container

// Fetch pangolins data
dbRef.on('value', snapshot => {
  container.innerHTML = ''; // Clear old cards

  if (!snapshot.exists()) {
    container.innerHTML = '<p>No pangolins found.</p>';
    return;
  }

  snapshot.forEach(pangolinSnap => {
    const pangolin = pangolinSnap.val();
    const id = pangolinSnap.key;

    const lastFed = pangolin.lastFed ? new Date(pangolin.lastFed).toLocaleString() : 'NOT FED YET';
    const growthStage = pangolin.growthStage || 'N/A';
    const happiness = pangolin.happiness !== undefined ? pangolin.happiness : 'N/A';
    const hunger = pangolin.hunger !== undefined ? pangolin.hunger : 'N/A';

    // Use the same HTML structure and classes as your previous design
    const card = document.createElement('div');
    card.className = 'pangolin-card'; // keep your original class
    card.innerHTML = `
      <h2 class="pangolin-title">${id.toUpperCase()}</h2>
      <p><strong>GROWTH:</strong> ${growthStage.toUpperCase()}</p>
      <p><strong>HAPPINESS:</strong> ${happiness}</p>
      <p><strong>HUNGER:</strong> ${hunger}</p>
      <p><strong>LAST FED:</strong> ${lastFed}</p>
    `;

    container.appendChild(card);
  });
});
