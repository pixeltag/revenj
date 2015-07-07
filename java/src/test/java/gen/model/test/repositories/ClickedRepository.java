package gen.model.test.repositories;



public class ClickedRepository   implements org.revenj.patterns.DomainEventStore<gen.model.test.Clicked> {
	
	
	
	public ClickedRepository(
			 final java.sql.Connection connection,
			 final org.revenj.postgres.ObjectConverter<gen.model.test.Clicked> converter,
			 final org.revenj.patterns.ServiceLocator locator) {
			
		
			this.connection = connection;
		
			this.converter = converter;
		
			this.locator = locator;
	}

	private final java.sql.Connection connection;
	private final org.revenj.postgres.ObjectConverter<gen.model.test.Clicked> converter;
	private final org.revenj.patterns.ServiceLocator locator;
	
	public ClickedRepository(org.revenj.patterns.ServiceLocator locator) {
		this(locator.resolve(java.sql.Connection.class), new org.revenj.patterns.Generic<org.revenj.postgres.ObjectConverter<gen.model.test.Clicked>>(){}.resolve(locator), locator);
	}
	
	//@Override
	private java.util.stream.Stream<gen.model.test.Clicked> stream(java.util.Optional<org.revenj.patterns.Specification<gen.model.test.Clicked>> filter) {
		throw new UnsupportedOperationException();
	}

	private java.util.ArrayList<gen.model.test.Clicked> readFromDb(java.sql.PreparedStatement statement, java.util.ArrayList<gen.model.test.Clicked> result) throws java.sql.SQLException, java.io.IOException {
		org.revenj.postgres.PostgresReader reader = new org.revenj.postgres.PostgresReader(locator::resolve);
		try (java.sql.ResultSet rs = statement.executeQuery()) {
			while (rs.next()) {
				org.postgresql.util.PGobject pgo = (org.postgresql.util.PGobject) rs.getObject(1);
				reader.process(pgo.getValue());
				result.add(converter.from(reader));
			}
		}
		return result;
	}

	@Override
	public java.util.List<gen.model.test.Clicked> search(java.util.Optional<org.revenj.patterns.Specification<gen.model.test.Clicked>> filter, java.util.Optional<Integer> limit, java.util.Optional<Integer> offset) {
		String sql = null;
		if (filter == null || !filter.isPresent() || filter.get() == null) {
			sql = "SELECT r FROM \"test\".\"Clicked_event\" r";
			if (limit != null && limit.isPresent() && limit.get() != null) {
				sql += " LIMIT " + Integer.toString(limit.get());
			}
			if (offset != null && offset.isPresent() && offset.get() != null) {
				sql += " OFFSET " + Integer.toString(offset.get());
			}
			try (java.sql.PreparedStatement statement = connection.prepareStatement(sql)) {
				return readFromDb(statement, new java.util.ArrayList<>());
			} catch (java.sql.SQLException | java.io.IOException e) {
				throw new RuntimeException(e);
			}
		}
		org.revenj.patterns.Specification<gen.model.test.Clicked> specification = filter.get();
		java.util.function.Consumer<java.sql.PreparedStatement> applyFilters = ps -> {};
		org.revenj.postgres.PostgresWriter pgWriter = new org.revenj.postgres.PostgresWriter();
		
		
		if (specification instanceof gen.model.test.Clicked.BetweenNumbers) {
			gen.model.test.Clicked.BetweenNumbers spec = (gen.model.test.Clicked.BetweenNumbers)specification;
			sql = "SELECT it FROM \"test\".\"Clicked.BetweenNumbers\"(?, ?) it";
			
			applyFilters = applyFilters.andThen(ps -> {
				try {
					ps.setBigDecimal(1, spec.getMin());
				} catch (Exception e) {
					throw new RuntimeException(e);
				}
			});
			applyFilters = applyFilters.andThen(ps -> {
				try {
					
					Object[] __arr = new Object[spec.getInSet().size()];
					int __ind = 0;
					for (Object __it : spec.getInSet()) __arr[__ind++] = __it;
					ps.setArray(2, connection.createArrayOf("numeric", __arr));
				} catch (Exception e) {
					throw new RuntimeException(e);
				}
			});
		}
		if (sql != null) {
			if (limit != null && limit.isPresent() && limit.get() != null) {
				sql += " LIMIT " + Integer.toString(limit.get());
			}
			if (offset != null && offset.isPresent() && offset.get() != null) {
				sql += " OFFSET " + Integer.toString(offset.get());
			}
			try (java.sql.PreparedStatement statement = connection.prepareStatement(sql)) {
				applyFilters.accept(statement);
				return readFromDb(statement, new java.util.ArrayList<>());
			} catch (java.sql.SQLException | java.io.IOException e) {
				throw new RuntimeException(e);
			}
		}
		java.util.stream.Stream<gen.model.test.Clicked> stream = stream(filter);
		if (offset != null && offset.isPresent() && offset.get() != null) {
			stream = stream.skip(offset.get());
		}
		if (limit != null && limit.isPresent() && limit.get() != null) {
			stream = stream.limit(limit.get());
		}
		return stream.collect(java.util.stream.Collectors.toList());
	}

	
	@Override
	public java.util.List<gen.model.test.Clicked> find(String[] uris) {
		try (java.sql.PreparedStatement statement = connection.prepareStatement("SELECT r FROM \"test\".\"Clicked_event\" r WHERE r._event_id = ANY(?)")) {
			Object[] ids = new Object[uris.length];
			for(int i = 0; i < uris.length; i++) {
				ids[i] = Long.parseLong(uris[i]);
			}
			statement.setArray(1, connection.createArrayOf("bigint", ids));
			return readFromDb(statement, new java.util.ArrayList<>(uris.length));			
		} catch (java.sql.SQLException | java.io.IOException e) {
			throw new RuntimeException(e);
		}
	}

	@Override
	public String[] submit(java.util.List<gen.model.test.Clicked> domainEvents) {
		try (java.sql.PreparedStatement statement = connection.prepareStatement("/*NO LOAD BALANCE*/SELECT \"URI\" FROM \"test\".\"submit_Clicked\"(?)")) {
			String[] result = new String[domainEvents.size()];
			org.revenj.postgres.PostgresWriter sw = new org.revenj.postgres.PostgresWriter();
			org.revenj.postgres.converters.PostgresTuple tuple = org.revenj.postgres.converters.ArrayTuple.create(domainEvents, converter::to);
			org.postgresql.util.PGobject pgo = new org.postgresql.util.PGobject();
			pgo.setType("\"test\".\"Clicked_event\"[]");
			tuple.buildTuple(sw, false);
			pgo.setValue(sw.toString());
			statement.setObject(1, pgo);
			try (java.sql.ResultSet rs = statement.executeQuery()) {
				for (int i = 0; i < result.length; i++) {
					rs.next();
					result[i] = rs.getString(1);
				}
			}
			return result;
		} catch (java.sql.SQLException e) {
			throw new RuntimeException(e);
		}
	}

	@Override
	public void mark(String[] uris) {
		try (java.sql.PreparedStatement statement = connection.prepareStatement("/*NO LOAD BALANCE*/SELECT \"test\".\"mark_Clicked\"(?)")) {
			Object[] ids = new Object[uris.length];
			for(int i = 0; i < uris.length; i++) {
				ids[i] = Long.parseLong(uris[i]);
			}
			statement.setArray(1, connection.createArrayOf("bigint", ids));
			statement.executeUpdate();
		} catch (java.sql.SQLException e) {
			throw new RuntimeException(e);
		}
	}

}