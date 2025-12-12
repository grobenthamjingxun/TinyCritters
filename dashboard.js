const firebaseConfig = {
  apiKey: "AIzaSyDQWz5awtRuIH586AEbTpcouopE-ahsCHE",
  authDomain: "asg1-bc97b.firebaseapp.com",
  databaseURL: "https://asg1-bc97b-default-rtdb.asia-southeast1.firebasedatabase.app",
  projectId: "asg1-bc97b",
  storageBucket: "asg1-bc97b.firebasestorage.app",
  messagingSenderId: "960117836452",
  appId: "1:960117836452:web:3e503125332a6d9d0bf431",
  measurementId: "G-T3FB9RQ90M"
};

// Initialize Firebase
firebase.initializeApp(firebaseConfig);

function updateDashboard(data, playerTag) {
    const growth = data.growth || 0;
    const inventoryArray = data.inventory || [];
    const inventory = inventoryArray.join(', ') || 'EMPTY';
    const lastFed = data.lastFed || 'NOT FED YET';

    document.getElementById('playerId').innerText = playerTag.toUpperCase();
    document.getElementById('growth').innerText = growth.toString().toUpperCase();
    document.getElementById('inventory').innerText = inventory.toUpperCase();
    document.getElementById('lastFed').innerText = lastFed.toUpperCase();
}

// Player tag
const playerTag = 'qIMhJcXPmOXZVSWNC6aocHuLV6J3';
const dbRef = firebase.database().ref('players/' + playerTag);

dbRef.on('value', snapshot => {
    if (snapshot.exists()) {
        updateDashboard(snapshot.val(), playerTag);
    } else {
        console.log("NO DATA AVAILABLE FOR PLAYER: " + playerTag.toUpperCase());
    }
});
