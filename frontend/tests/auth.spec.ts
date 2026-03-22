import { test, expect } from "@playwright/test";

// unique username per run to avoid conflicts
const uid = () => `t${Date.now()}${Math.floor(Math.random() * 1000)}`;

test.describe("login screen", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/");
  });

  test("shows login form on load", async ({ page }) => {
    await expect(page.getByRole("heading", { name: "conquerio" })).toBeVisible();
    await expect(page.getByPlaceholder("username")).toBeVisible();
    await expect(page.getByPlaceholder("password")).toBeVisible();
    await expect(page.getByRole("button", { name: "play" })).toBeVisible();
  });

  test("toggle to register form shows email field", async ({ page }) => {
    await page.getByText("no account? register").click();
    await expect(page.getByPlaceholder("email")).toBeVisible();
    await expect(page.getByRole("button", { name: "register" })).toBeVisible();
  });

  test("toggle back to login hides email field", async ({ page }) => {
    await page.getByText("no account? register").click();
    await page.getByText("have an account? login").click();
    await expect(page.getByPlaceholder("email")).not.toBeVisible();
    await expect(page.getByRole("button", { name: "play" })).toBeVisible();
  });
});

test.describe("registration validation", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/");
    await page.getByText("no account? register").click();
  });

  test("password too short", async ({ page }) => {
    await page.getByPlaceholder("username").fill("someuser");
    await page.getByPlaceholder("email").fill("x@test.com");
    await page.getByPlaceholder("password").fill("ab1");
    await page.getByRole("button", { name: "register" }).click();
    await expect(page.getByText("Password must be at least 6 characters")).toBeVisible();
  });

  test("password missing digit", async ({ page }) => {
    await page.getByPlaceholder("username").fill("someuser");
    await page.getByPlaceholder("email").fill("x@test.com");
    await page.getByPlaceholder("password").fill("abcdef");
    await page.getByRole("button", { name: "register" }).click();
    await expect(page.getByText("Password must contain at least one digit")).toBeVisible();
  });

  test("password missing lowercase", async ({ page }) => {
    await page.getByPlaceholder("username").fill("someuser");
    await page.getByPlaceholder("email").fill("x@test.com");
    await page.getByPlaceholder("password").fill("ABCDEF1");
    await page.getByRole("button", { name: "register" }).click();
    await expect(page.getByText("Password must contain at least one lowercase letter")).toBeVisible();
  });
});

test.describe("auth flow", () => {
  test("login with wrong password shows error", async ({ page }) => {
    await page.goto("/");
    await page.getByPlaceholder("username").fill("definitlynotauser99999");
    await page.getByPlaceholder("password").fill("wrongpass1");
    await page.getByRole("button", { name: "play" }).click();
    await expect(page.getByText(/wrong username or password/i)).toBeVisible({ timeout: 10_000 });
  });

  test("register then auto-login lands on room browser", async ({ page }) => {
    const user = uid();
    await page.goto("/");
    await page.getByText("no account? register").click();
    await page.getByPlaceholder("username").fill(user);
    await page.getByPlaceholder("email").fill(`${user}@test.com`);
    await page.getByPlaceholder("password").fill("testpass1");
    await page.getByRole("button", { name: "register" }).click();
    await expect(page.getByRole("button", { name: "quick play" })).toBeVisible({ timeout: 10_000 });
  });

  test("login with existing user lands on room browser", async ({ page }) => {
    // register first
    const user = uid();
    await page.goto("/");
    await page.getByText("no account? register").click();
    await page.getByPlaceholder("username").fill(user);
    await page.getByPlaceholder("email").fill(`${user}@test.com`);
    await page.getByPlaceholder("password").fill("testpass1");
    await page.getByRole("button", { name: "register" }).click();
    await expect(page.getByRole("button", { name: "quick play" })).toBeVisible({ timeout: 10_000 });

    // logout and log back in
    await page.getByRole("button", { name: "logout" }).click();
    await page.getByPlaceholder("username").fill(user);
    await page.getByPlaceholder("password").fill("testpass1");
    await page.getByRole("button", { name: "play" }).click();
    await expect(page.getByRole("button", { name: "quick play" })).toBeVisible({ timeout: 10_000 });
  });
});
