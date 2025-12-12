// The player ID you showed me
const PLAYER_ID = "qIMhJcXPmOXZVSWNC6aocHuLV6J3";

const playerRef = firebase.database().ref("players/" + PLAYER_ID);

// Realtime listener
playerRef.on("value", snapshot => {
    const data = snapshot.val();

    if (!data) {
        console.log("No player data found");
        return;
    }

    // Update growth
    document.getElementById("growth").textContent = data.growth ?? "N/A";

    // Update inventory
    const invDiv = document.getElementById("inventory-list");
    invDiv.innerHTML = "";

    if (data.inventory) {
        Object.values(data.inventory).forEach(item => {
            const span = document.createElement("span");
            span.textContent = item;
            invDiv.appendChild(span);
        });
    } else {
        invDiv.textContent = "No items";
    }

    // Update lastFed
    document.getElementById("lastFed").textContent =
        data.lastFed && data.lastFed !== "" ? data.lastFed : "Never fed yet.";
});
