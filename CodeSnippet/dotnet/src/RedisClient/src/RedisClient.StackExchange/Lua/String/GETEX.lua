local key = KEYS[1]
local expiry = ARGV[1]
if (expiry == 'PERSIST') then
    return redis.pcall('GETEX', key, expiry)
else
    local expiryValue = ARGV[2]
    return redis.pcall('GETEX', key, expiry, expiryValue)
end
