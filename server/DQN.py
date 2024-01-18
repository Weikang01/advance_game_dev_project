import torch


class DQN:
    class net(torch.nn.Module):
        def __init__(self, input_shape, num_actions, *args, **kwargs):
            super().__init__(*args, **kwargs)
            self.input_shape = input_shape
            self.num_actions = num_actions

            self.conv = torch.nn.Sequential(
                torch.nn.Conv2d(input_shape[0], 32, kernel_size=8, stride=4),
                torch.nn.ReLU(),
                torch.nn.Conv2d(32, 64, kernel_size=4, stride=2),
                torch.nn.ReLU(),
                torch.nn.Conv2d(64, 64, kernel_size=3, stride=1),
                torch.nn.ReLU()
            )

            conv_out_size = self._get_conv_out(input_shape)

            self.fc = torch.nn.Sequential(
                torch.nn.Linear(conv_out_size, 512),
                torch.nn.ReLU(),
                torch.nn.Linear(512, num_actions)
            )

        def _get_conv_out(self, shape):
            o = self.conv(torch.zeros(1, *shape))
            return int(torch.prod(torch.tensor(o.size())))

        def forward(self, x):
            conv_out = self.conv(x).view(x.size()[0], -1)
            return self.fc(conv_out)

    def __init__(self, input_shape, num_actions, *args, **kwargs):
        self.net = self.net(input_shape, num_actions, *args, **kwargs)
        self.target_net = self.net(input_shape, num_actions, *args, **kwargs)
        self.target_net.load_state_dict(self.net.state_dict())
        self.target_net.eval()
        self.optimizer = torch.optim.Adam(self.net.parameters(), lr=0.0001)
        self.loss = torch.nn.SmoothL1Loss()

    def update(self, batch):
        states, actions, rewards, next_states, dones = batch

        states = torch.tensor(states, dtype=torch.float32)
        actions = torch.tensor(actions, dtype=torch.long)
        rewards = torch.tensor(rewards, dtype=torch.float32)
        next_states = torch.tensor(next_states, dtype=torch.float32)
        dones = torch.tensor(dones, dtype=torch.bool)

        state_action_values = self.net(states).gather(1, actions.unsqueeze(-1)).squeeze(-1)
        next_state_values = self.target_net(next_states).max(1)[0].detach()
        next_state_values[dones] = 0.0
        expected_state_action_values = (next_state_values * 0.999) + rewards

        loss = self.loss(state_action_values, expected_state_action_values)
        self.optimizer.zero_grad()
        loss.backward()
        self.optimizer.step()

    def get_action(self, state, epsilon):
        if torch.rand(1) < epsilon:
            return torch.randint(0, self.net.num_actions, (1,))
        else:
            with torch.no_grad():
                return self.net(torch.tensor(state, dtype=torch.float32).unsqueeze(0)).max(1)[1].view(1, 1)
