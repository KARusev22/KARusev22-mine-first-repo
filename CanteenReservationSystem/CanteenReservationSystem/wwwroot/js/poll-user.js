function scrollPolls(direction) {
    const container = document.getElementById("pollScroll");
    const amount = 400;
    container.scrollLeft += direction * amount;
}

function submitVote(pollId) {
    const selected = document.querySelector(`input[name="option-${pollId}"]:checked`);
    if (!selected) {
        alert("Please select an option");
        return;
    }

    const optionId = selected.value;

    fetch("/Poll/SubmitVote", {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded"
        },
        body: `optionId=${optionId}`
    })
        .then(response => {
            if (!response.ok) {
                if (response.status === 400) {
                    alert("You have already voted.");
                }
                return;
            }

            const card = document.querySelector(`[data-poll-id="${pollId}"]`);

            const options = card.querySelector(".poll-options");
            const voteBtn = card.querySelector(".poll-vote-btn");
            if (options) options.remove();
            if (voteBtn) voteBtn.remove();

            const thankYou = document.createElement("div");
            thankYou.className = "poll-voted-label";
            thankYou.innerText = "Thank you for voting!";
            card.appendChild(thankYou);

            const resultsBtn = document.createElement("a");
            resultsBtn.className = "poll-results-btn";
            resultsBtn.innerText = "View Results";
            resultsBtn.href = `/AdminPoll/Results/${pollId}`;
            card.appendChild(resultsBtn);
        });
}