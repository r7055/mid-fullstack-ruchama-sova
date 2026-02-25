import { NavLink, Navigate, Route, Routes } from "react-router-dom";
import { API_BASE_URL } from "./lib/config";
import { AddTransactionsPage } from "./routes/AddTransactionsPage";
import { MonitorPage } from "./routes/MonitorPage";

export default function App() {
  return (
    <div className="app-shell">
      <header className="app-header">
        <div>
          <p className="eyebrow">MID Fullstack Project</p>
          <h1>Real-Time Transaction Control</h1>
          <p className="backend-tag">Backend: {API_BASE_URL}</p>
        </div>
        <nav className="top-nav">
          <NavLink
            to="/add"
            className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}
          >
            /add Simulator
          </NavLink>
          <NavLink
            to="/monitor"
            className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}
          >
            /monitor Live Board
          </NavLink>
        </nav>
      </header>

      <main className="app-main">
        <Routes>
          <Route path="/" element={<Navigate to="/monitor" replace />} />
          <Route path="/add" element={<AddTransactionsPage />} />
          <Route path="/monitor" element={<MonitorPage />} />
        </Routes>
      </main>
    </div>
  );
}
