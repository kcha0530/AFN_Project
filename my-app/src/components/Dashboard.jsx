import Counter from "./Counter.jsx";
import ToggleText from "./ToggleText.jsx";
import ItemList from "./ItemList.jsx";
import LikeButton from "./LikeButton.jsx";
import GitHubProfile from "./GitHubProfile.jsx";
import RandomJoke from "./RandomJoke.jsx";
import BackendProducts from "./BackendProducts.jsx";

function Dashboard({ userName }) {
  return (
    <main>
      <section className="dashboard-welcome card" id="dashboard">
        <h2>Welcome, {userName}!</h2>
        <p>
          You are successfully authenticated. This dashboard is only available after login.
          Explore the interactive components and the secure backend product feed.
        </p>
      </section>

      <section className="grid two-column">
        <Counter />
        <ToggleText />
        <ItemList />
        <LikeButton />
      </section>

      <section className="grid two-column">
        <GitHubProfile />
        <RandomJoke />
      </section>

      <section className="grid">
        <BackendProducts />
      </section>
    </main>
  );
}

export default Dashboard;
