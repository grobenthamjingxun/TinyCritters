# TinyCritters 🐾

## Project Overview

**TinyCritters** is an educational Augmented Reality (AR) mobile game designed to introduce children to **endangered animals in Singapore** through interactive gameplay. Players adopt and care for virtual animals using real-world interactions, learning about conservation while engaging with game mechanics powered by **Unity**, **Firebase**, and a **real-time web dashboard**.

The project is developed as a **Full Stack DDA (Digital Development Assignment)**, demonstrating end-to-end integration between a Unity AR application, Firebase backend services, and a web-based administrative dashboard.

---

## Core Concept

Players adopt a Singapore-based endangered animal, such as:

* **Sunda Pangolin**
* **Raffles’ Banded Langur**
* **Sunda Slow Loris**

Using AR, players interact with their chosen animal by feeding it, entertaining it, and observing its growth and reactions. Each interaction is designed to educate players about the animal’s natural diet, behavior, and conservation status.

---

## Key Features

### 1. Augmented Reality Gameplay (Unity)

* Image tracking to spawn animals in real-world environments
* Drag-and-drop interaction system
* Real-time visual feedback through animations and scaling
* Child-friendly and intuitive user interface

### 2. Feeding & Interaction System

* Scan or select fruits and vegetables to feed animals
* Correct foods increase growth and trigger positive reactions
* Incorrect foods result in negative reactions and reduced growth progression
* Universal entertainment items (e.g., toys) boost growth speed for all animals

### 3. Educational Focus

* In-game animal facts displayed during interactions
* Conservation awareness integrated into gameplay
* Focus on lesser-known endangered animals in Singapore
* Learning through play rather than text-heavy explanations

---

## Firebase Integration

### Authentication

* Firebase Email/Password Authentication
* User sign-up, login, and logout
* User-friendly error handling for authentication issues

### Realtime Database

* Player profiles and progress stored persistently
* Adopted animal data (type, growth, interactions)
* Real-time synchronization between Unity and web dashboard
* Sample data included for testing and demonstration

### Data Handling

* Sensible and structured JSON-based data hierarchy
* In-game actions immediately update Firebase
* Data persists across sessions

---

## Web Dashboard

The project includes a **web-based dashboard** that acts as an extension and administrative interface for the game.

### Dashboard Features

* Deployed web frontend connected to Firebase Realtime Database
* Real-time listeners for live data updates
* Display of player profiles and adopted animals
* Monitoring of animal growth and interaction data
* Administrative data management
* Responsive design for different screen sizes

---

## UI / UX Design

* Clear navigation flow across Unity and web interfaces
* Purposeful placement of UI elements
* Consistent visual feedback for user actions
* Friendly, approachable design suitable for children
* Error messages that are easy to understand

---

## Testing & Stability

The project has undergone comprehensive testing to ensure stability and usability:

* Authentication testing (login, signup, logout)
* Firebase database read/write testing
* AR interaction and image tracking testing
* User input validation testing
* End-to-end testing across Unity, Firebase, and web dashboard

---

## Promotional Materials

* Promotional social media posts created to showcase the game concept and features
* AI-generated promotional video used to visually communicate gameplay and educational value

---

## Target Audience

The primary target audience is **children**, with the goal of:

* Encouraging interest in animals and nature
* Raising awareness of endangered species in Singapore
* Promoting conservation efforts supported by organizations such as **Mandai Wildlife Group**

By combining AR, game mechanics, and real-time data systems, TinyCritters aims to make learning engaging, interactive, and memorable.

---

## Technology Stack

* **Unity** – AR game development
* **Firebase Authentication** – User management
* **Firebase Realtime Database** – Data storage and synchronization
* **HTML / CSS / JavaScript** – Web dashboard

---

## Project Status

✅ Fully functional full-stack DDA project
✅ Unity + Firebase + Web dashboard integrated
✅ Educational and engagement goals achieved

---

## Conclusion

TinyCritters demonstrates a complete **Full Stack AR game solution** that blends education, engagement, and modern web technologies. The project showcases real-time data interaction, meaningful gameplay mechanics, and a strong focus on user experience and learning outcomes.

**TinyCritters – Learn, Care, Protect.** 🌱
