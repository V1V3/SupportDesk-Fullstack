import { useEffect, useState } from "react";

function TicketsPage() {
    const [message, setMessage] = useState("Loading...");
    const [error, setError] = useState("");

    useEffect(() => {
        async function fetchHealth() {
            try {
                const response = await fetch("http://localhost:5270/api/health");

                if (!response.ok) {
                    throw new Error("Failed to reach API.");
                }

                const data = await response.json();
                setMessage(data.message);
            } catch (err) {
                setError(err.message);
            }
        }

        fetchHealth();
    }, []);

    return (
        <div>
            <h2>Tickets</h2>
            <p>This is the tickets dashboard.</p>

            {error ? <p>Error: {error}</p> : <p>API message: {message}</p>}
        </div>
    );
}

export default TicketsPage;