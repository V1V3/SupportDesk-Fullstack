import { BrowserRouter, Routes, Route, Link } from "react-router-dom";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import TicketsPage from "./pages/TicketsPage";

function App() {
    return (
        <BrowserRouter>
            <div>
                <nav style={{ padding: "1rem", borderBottom: "1px solid #ccc" }}>
                    <Link to="/login" style={{ marginRight: "1rem" }}>Login</Link>
                    <Link to="/register" style={{ marginRight: "1rem" }}>Register</Link>
                    <Link to="/tickets">Tickets</Link>
                </nav>

                <main style={{ padding: "1rem" }}>
                    <Routes>
                        <Route path="/login" element={<LoginPage />} />
                        <Route path="/register" element={<RegisterPage />} />
                        <Route path="/tickets" element={<TicketsPage />} />
                        <Route path="*" element={<LoginPage />} />
                    </Routes>
                </main>
            </div>
        </BrowserRouter>
    );
}

export default App;