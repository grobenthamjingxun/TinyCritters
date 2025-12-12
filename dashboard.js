// Firebase configuration
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
const dbRef = firebase.database().ref('players/qIMhJcXPmOXZVSWNC6aocHuLV6J3');

// Update dashboard UI
function updateDashboard(data) {
    const growth = data.growth || 0;
    const inventoryArray = data.inventory || [];
    const inventory = inventoryArray.join(', ') || 'Empty';
    const lastFed = data.lastFed || 'Not fed yet';

    document.getElementById('growth').innerText = growth;
    document.getElementById('inventory').innerText = inventory;
    document.getElementById('lastFed').innerText = lastFed;
}

// Listen for changes in real-time
dbRef.on('value', snapshot => {
    if (snapshot.exists()) {
        updateDashboard(snapshot.val());
    } else {
        console.log("No data available");
    }
});
